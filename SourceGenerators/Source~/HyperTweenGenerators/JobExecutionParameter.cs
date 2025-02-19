using System;
using System.Linq;
using Microsoft.CodeAnalysis;
using Unity.Entities.SourceGen.Common;

namespace HyperTweenGenerators;

public class JobExecutionParameter(IParameterSymbol parameterSymbol, ITypeSymbol invokeComponentSymbol)
{
    private enum Type
    {
        UnmanagedReadOnlyDirectComponent,
        ManagedReadOnlyDirectComponent,
        ReadOnlyIndirectComponent,
        UnmanagedDirectComponent,
        ManagedDirectComponent,
        IndirectComponent,
        TweenEntity,
        TargetEntity,
        EntityCommandBuffer,
        ParallelEntityCommandBuffer,
        TweenFactory,
        ParallelTweenFactory
    }

    private readonly Type _type = GetType(parameterSymbol, invokeComponentSymbol);

    private static Type GetType(IParameterSymbol parameterSymbol, ITypeSymbol invokeComponentSymbol)
    {
        var fullName = parameterSymbol.Type.GetFullName();
        
        if (fullName == "Unity.Entities.ComponentLookup")
        {
            return parameterSymbol.RefKind switch
            {
                RefKind.In => Type.ReadOnlyIndirectComponent,
                RefKind.Ref => Type.IndirectComponent,
                _ => throw new InvalidOperationException("Component parameters must either be in or ref.")
            };
        }

        if (fullName == invokeComponentSymbol.GetFullName())
        {
            throw new InvalidOperationException($"Do not pass {fullName} as a parameter. The Invoke method is called on the instance of the component." +
                                                $"You can access this component data using `this`. If a write to one of the fields is detected the system" +
                                                $"will write the component data to the chunk after Invoke returns");
        }

        if(parameterSymbol.Type.TryGetImplementedInterface("Unity.Entities.IComponentData", out _))
        {
            return (parameterSymbol.RefKind, parameterSymbol.Type.TypeKind) switch
            {
                (RefKind.In, TypeKind.Struct) => Type.UnmanagedReadOnlyDirectComponent,
                (RefKind.In, TypeKind.Class) => Type.ManagedReadOnlyDirectComponent,
                (RefKind.Ref, TypeKind.Struct) => Type.UnmanagedDirectComponent,
                (RefKind.Ref, TypeKind.Class) => Type.ManagedDirectComponent,
                _ => throw new InvalidOperationException("Component parameters must either be in or ref.")
            };
        }

        if(fullName == "Unity.Entities.Entity")
        {
            return parameterSymbol.Name switch
            {
                "tweenEntity" => Type.TweenEntity,
                "targetEntity" => Type.TargetEntity,
                _ => throw new InvalidOperationException("Entity parameters must either be named tweenEntity or targetEntity.")
            };
        }
        if (fullName == "Unity.Entities.EntityCommandBuffer")
        {
            return parameterSymbol.RefKind switch
            {
                RefKind.None => Type.EntityCommandBuffer,
                _ => throw new InvalidOperationException("No RefKind expected for EntityCommandBuffer.")
            };
        } 
        
        if (fullName == "Unity.Entities.EntityCommandBuffer.ParallelWriter")
        {
            return parameterSymbol.RefKind switch
            {
                RefKind.None => Type.ParallelEntityCommandBuffer,
                _ => throw new InvalidOperationException("No RefKind expected for EntityCommandBuffer.")
            };
        }
        
        if (fullName == "HyperTween.API.TweenFactory")
        {
            if (!parameterSymbol.Type.TryGetGenericTypeArguments(out var typeArgumentSymbols))
            {
                throw new InvalidOperationException("Expected TweenFactory to have generic args");
            }
            
            return (parameterSymbol.RefKind, typeArgumentSymbols.First().GetFullName()) switch
            {
                (RefKind.None,"HyperTween.TweenBuilders.EntityCommandBufferTweenBuilder") => Type.TweenFactory,
                (RefKind.None,"HyperTween.TweenBuilders.EntityCommandBufferParallelWriterTweenBuilder")  => Type.ParallelTweenFactory,
                _ => throw new InvalidOperationException("No RefKind expected for TweenFactory and TweenFactory type must be EntityCommandBufferTweenBuilder or EntityCommandBufferParallelWriterTweenBuilder.")
            };
        }

        throw new ArgumentException($"Unable to determine what type this parameter is: {parameterSymbol.Name}");
    }

    public string GetUsing(int index)
    {
        if (_type == Type.TargetEntity)
        {
            return "using HyperTween.ECS.Update.Components;";
        }
        
        return $"using {parameterSymbol.ContainingNamespace.ToFullName()};";
    }
    
    public string GetJobDataDefinition(int index)
    {
        switch (_type)
        {
            case Type.UnmanagedReadOnlyDirectComponent:
            case Type.ManagedReadOnlyDirectComponent:
                return $"[ReadOnly] public ComponentTypeHandle<{parameterSymbol.Type.GetFullName()}> {parameterSymbol.Name}_{index}_TypeHandle;";
            case Type.ReadOnlyIndirectComponent:
                return $"[ReadOnly] public ComponentLookup<{parameterSymbol.Type.GetFullName()}> {parameterSymbol.Name}_{index}_Lookup;";
            case Type.UnmanagedDirectComponent:
            case Type.ManagedDirectComponent:
                return $"public ComponentTypeHandle<{parameterSymbol.Type.GetFullName()}>{parameterSymbol.Name}_{index}_TypeHandle;";
            case Type.IndirectComponent:
                return $"public ComponentLookup<{parameterSymbol.Type.GetFullName()}> {parameterSymbol.Name}_{index}_Lookup;";
            case Type.TweenEntity:
                return "// TweenEntity no JobData definition required";
            case Type.TargetEntity:
                return "// TargetEntity no JobData definition required";
            case Type.EntityCommandBuffer:
                return $"public EntityCommandBuffer {parameterSymbol.Name}_{index}_ECB;";
            case Type.ParallelEntityCommandBuffer:
                return $"public EntityCommandBuffer.ParallelWriter {parameterSymbol.Name}_{index}_ECBPW;";
            case Type.TweenFactory:
                return $"public TweenFactory<EntityCommandBufferTweenBuilder> {parameterSymbol.Name}_{index}_TweenFactory;";
            case Type.ParallelTweenFactory:
                return $"public TweenFactory<EntityCommandBufferParallelWriterTweenBuilder> {parameterSymbol.Name}_{index}_ParallelTweenFactory;";
            default:
                throw new ArgumentOutOfRangeException();
        }
    }
    
    public string? GetQueryType(int index)
    {
        switch (_type)
        {
            case Type.UnmanagedReadOnlyDirectComponent:
            case Type.UnmanagedDirectComponent:
            case Type.ManagedReadOnlyDirectComponent:
            case Type.ManagedDirectComponent:
                return parameterSymbol.Type.GetFullName();
            default:
                return null;
        }
    }
    
    public string GetNativeArray(int index)
    {
        switch (_type)
        {
            case Type.UnmanagedReadOnlyDirectComponent:
            case Type.UnmanagedDirectComponent:
                return $"var {parameterSymbol.Name}_{index}_Array = chunk.GetNativeArray(ref jobData.{parameterSymbol.Name}_{index}_TypeHandle);";
            case Type.ManagedReadOnlyDirectComponent:
            case Type.ManagedDirectComponent:
                return $"var {parameterSymbol.Name}_{index}_Array = chunk.GetManagedComponentAccessor(ref jobData.{parameterSymbol.Name}_{index}_TypeHandle, state.EntityManager);";
            case Type.ReadOnlyIndirectComponent:
                return $"var {parameterSymbol.Name}_{index}_Lookup = jobData.{parameterSymbol.Name}_{index}_Lookup";
            case Type.IndirectComponent:
                return $"var {parameterSymbol.Name}_{index}_Lookup = jobData.{parameterSymbol.Name}_{index}_Lookup";
            case Type.TweenEntity:
                return "// TweenEntity no GetNativeArray definition required";
            case Type.TargetEntity:
                return "// TargetEntity no GetNativeArray definition required";
            case Type.EntityCommandBuffer:
                return $"var {parameterSymbol.Name}_{index}_ECB = jobData.{parameterSymbol.Name}_{index}_ECB;";
            case Type.ParallelEntityCommandBuffer:
                return $"var {parameterSymbol.Name}_{index}_ECB = jobData.{parameterSymbol.Name}_{index}_ECBPW;";
            case Type.TweenFactory:
                return $"var {parameterSymbol.Name}_{index}_TweenFactory = jobData.{parameterSymbol.Name}_{index}_TweenFactory;";
            case Type.ParallelTweenFactory:
                return $"var {parameterSymbol.Name}_{index}_ParallelTweenFactory = jobData.{parameterSymbol.Name}_{index}_ParallelTweenFactory;";
            default:
                throw new ArgumentOutOfRangeException();
        }
    }
    
    public string GetRead(int index)
    {
        switch (_type)
        {
            case Type.UnmanagedReadOnlyDirectComponent:
            case Type.UnmanagedDirectComponent:
            case Type.ManagedReadOnlyDirectComponent:
            case Type.ManagedDirectComponent:
                return $"var {parameterSymbol.Name}_{index} = {parameterSymbol.Name}_{index}_Array[i];";
            case Type.ReadOnlyIndirectComponent:
                return "// No Read For ReadOnlyIndirectComponent";
            case Type.IndirectComponent:
                return "// No Read For IndirectComponent";
            case Type.TweenEntity:
                return "// No Read For TweenEntity";
            case Type.TargetEntity:
                return "// No Read For TargetEntity";
            case Type.EntityCommandBuffer:
                return "// No Read For ECB";
            case Type.ParallelEntityCommandBuffer:
                return "// No Read For ECBPW";
            case Type.TweenFactory:
                return "// No Read For TweenFactory";
            case Type.ParallelTweenFactory:
                return "// No Read For ParallelTweenFactory";
            default:
                throw new ArgumentOutOfRangeException();
        }
    }
    
    public string GetJobInvokeParam(int index)
    {
        return _type switch
        {
            Type.TweenEntity => "tweenEntity",
            Type.TargetEntity => "targetEntity",
            Type.UnmanagedReadOnlyDirectComponent => $"in {parameterSymbol.Name}_{index}",
            Type.ManagedReadOnlyDirectComponent => $"in {parameterSymbol.Name}_{index}",
            Type.UnmanagedDirectComponent => $"ref {parameterSymbol.Name}_{index}",
            Type.ManagedDirectComponent => $"ref {parameterSymbol.Name}_{index}",
            Type.EntityCommandBuffer => $"{parameterSymbol.Name}_{index}_ECB",
            Type.ParallelEntityCommandBuffer => $"{parameterSymbol.Name}_{index}_ECBPW",
            Type.ReadOnlyIndirectComponent => $"{parameterSymbol.Name}_{index}_Lookup",
            Type.IndirectComponent => $"{parameterSymbol.Name}_{index}_Lookup",
            Type.TweenFactory => $"{parameterSymbol.Name}_{index}_TweenFactory",
            Type.ParallelTweenFactory => $"{parameterSymbol.Name}_{index}_ParallelTweenFactory",
            _ => throw new ArgumentOutOfRangeException()
        };
    }
    
    public string? GetWrite(int index)
    {
        switch (_type)
        {
            case Type.UnmanagedDirectComponent:
                return $"{parameterSymbol.Name}_{index}_Array[i] = {parameterSymbol.Name}_{index};";
            default:
                return null;
        }
    }
    
    public string GetInitialiseJobData(int index)
    {
        switch (_type)
        {
            case Type.UnmanagedReadOnlyDirectComponent:
            case Type.ManagedReadOnlyDirectComponent:
                return $"_jobData.{parameterSymbol.Name}_{index}_TypeHandle = state.GetComponentTypeHandle<{parameterSymbol.Type.GetFullName()}>();";
            case Type.ReadOnlyIndirectComponent:
                return $"_jobData.{parameterSymbol.Name}_{index}_Lookup = state.GetComponentLookup<{parameterSymbol.Type.GetFullName()}>();";
            case Type.UnmanagedDirectComponent:
            case Type.ManagedDirectComponent:
                return $"_jobData.{parameterSymbol.Name}_{index}_TypeHandle = state.GetComponentTypeHandle<{parameterSymbol.Type.GetFullName()}>();";
            case Type.IndirectComponent:
                return $"_jobData.{parameterSymbol.Name}_{index}_Lookup = state.GetComponentLookup<{parameterSymbol.Type.GetFullName()}>();";
            case Type.TweenEntity:
                return $"// TweenEntity no GetInitialiseJobData definition required";
            case Type.TargetEntity:
                return "// TargetEntity no GetInitialiseJobData definition required";
            case Type.EntityCommandBuffer:
            case Type.ParallelEntityCommandBuffer:
            case Type.TweenFactory:
            case Type.ParallelTweenFactory:
                return string.Empty;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }
    
    public string GetUpdateJobData(int index)
    {
        switch (_type)
        {
            case Type.UnmanagedReadOnlyDirectComponent:
                return $"_jobData.{parameterSymbol.Name}_{index}_TypeHandle.Update(ref state);";
            case Type.ReadOnlyIndirectComponent:
                return $"_jobData.{parameterSymbol.Name}_{index}_Lookup.Update(ref state);";
            case Type.UnmanagedDirectComponent:
                return $"_jobData.{parameterSymbol.Name}_{index}_TypeHandle.Update(ref state);";
            case Type.IndirectComponent:
                return $"_jobData.{parameterSymbol.Name}_{index}_Lookup.Update(ref state);";
            case Type.TweenEntity:
                return $"// TweenEntity no GetUpdateJobData definition required";
            case Type.TargetEntity:
                return "// TargetEntity no GetUpdateJobData definition required";
            case Type.EntityCommandBuffer:
                return $"_jobData.{parameterSymbol.Name}_{index}_ECB = ecb;";
            case Type.ParallelEntityCommandBuffer:
                return $"_jobData.{parameterSymbol.Name}_{index}_ECBPW = ecb.AsParallelWriter();";
            case Type.TweenFactory:
                return $"_jobData.{parameterSymbol.Name}_{index}_TweenFactory = ecb.CreateTweenFactory(world);";
             case Type.ParallelTweenFactory:
                return $"_jobData.{parameterSymbol.Name}_{index}_ParallelTweenFactory = ecb.CreateTweenFactory(world).AsParallelWriter();";
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    public bool AllowParallel()
    {
        return _type switch
        {
            Type.UnmanagedReadOnlyDirectComponent => true,
            Type.ReadOnlyIndirectComponent => true,
            Type.UnmanagedDirectComponent => true,
            Type.IndirectComponent => false,
            Type.TweenEntity => true,
            Type.TargetEntity => true,
            Type.EntityCommandBuffer => false,
            Type.ParallelEntityCommandBuffer => true,
            Type.TweenFactory => false,
            _ => throw new ArgumentOutOfRangeException()
        };
    }
}