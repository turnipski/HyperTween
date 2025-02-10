using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Unity.Entities.SourceGen.Common;

namespace HyperTweenGenerators;

[Generator]
public class TweenOutputSystemGenerator : ISourceGenerator
{
    private const string TweenToSymbolName = "HyperTween.ECS.Update.Components.ITweenTo";

    public void Initialize(GeneratorInitializationContext context)
    {
    }

    public void Execute(GeneratorExecutionContext context)
    {
        if (!SourceGenHelpers.IsBuildTime || !SourceGenHelpers.ShouldRun(context.Compilation, context.CancellationToken))
            return;

        SourceOutputHelpers.Setup(context.ParseOptions, context.AdditionalFiles);

        Location? lastLocation = null;
        SourceOutputHelpers.LogInfoToSourceGenLog($"Source generating assembly {context.Compilation.Assembly.Name}...");

        var stopwatch = Stopwatch.StartNew();

        try
        {
            var compilation = context.Compilation;
            var syntaxTrees = compilation.SyntaxTrees;
            
            var syntaxTreeToGeneratedSourceMap = new Dictionary<SyntaxTree, List<string>>();

            foreach (var syntaxTree in syntaxTrees)
            {
                var semanticModel = compilation.GetSemanticModel(syntaxTree);
                
                var structSymbols = syntaxTree.GetRoot()
                    .DescendantNodes()
                    .OfType<StructDeclarationSyntax>()
                    .Select(syntax => semanticModel.GetDeclaredSymbol(syntax))
                    .Cast<ITypeSymbol>();
                
                var classSymbols = syntaxTree.GetRoot()
                    .DescendantNodes()
                    .OfType<ClassDeclarationSyntax>()
                    .Select(syntax => semanticModel.GetDeclaredSymbol(syntax))
                    .Cast<ITypeSymbol>();

                var allSymbols = structSymbols.Concat(classSymbols);

                foreach (var symbol in structSymbols)
                {
                    if (!symbol.TryGetImplementedInterface(TweenToSymbolName, out var interfaceSymbol))
                    {
                        continue;
                    }

                    syntaxTreeToGeneratedSourceMap.Add(syntaxTree, GenerateOutputSystemsAndComponents(symbol, interfaceSymbol).ToList());
                }
            }

            // Generate source files in parallel for debugging purposes (very useful to be able to visually inspect generated code!).
            // Add generated source to compilation only if there are no errors.
            foreach (var kvp in syntaxTreeToGeneratedSourceMap)
            {
                var syntaxTree = kvp.Key;
                var generatedPartialSystems = kvp.Value;

                for (int i = 0; i < generatedPartialSystems.Count; i++)
                {
                    var generatedPartial = generatedPartialSystems[i];
                    var generatedFile = syntaxTree.GetGeneratedSourceFilePath(context.Compilation.Assembly.Name, nameof(TweenOutputSystemGenerator), salting: i);
                    var outputSource = TypeCreationHelpers.FixUpLineDirectivesAndOutputSource(generatedFile.FullFilePath, generatedPartial);
                    
                    context.AddSource(generatedFile.FileNameOnly, outputSource);
                    
                    SourceOutputHelpers.OutputSourceToFile(
                        generatedFile.FullFilePath,
                        () => outputSource.ToString());
                }
            }

            stopwatch.Stop();

            SourceOutputHelpers.LogInfoToSourceGenLog($"TIME : {nameof(TweenOutputSystemGenerator)} : {context.Compilation.Assembly.Name} : {stopwatch.ElapsedMilliseconds}ms");
        }
        catch (Exception exception)
        {
            if (exception is OperationCanceledException)
                throw;

            context.LogError("SGICE002", nameof(TweenOutputSystemGenerator), exception.ToUnityPrintableString(), lastLocation ?? context.Compilation.SyntaxTrees.First().GetRoot().GetLocation());
        }
    }

    private IEnumerable<string> GenerateOutputSystemsAndComponents(ITypeSymbol componentSymbol, ITypeSymbol tweenToInterfaceSymbol)
    {
        if (!tweenToInterfaceSymbol.TryGetGenericTypeArguments(out var typeArgumentSymbols))
        {
            throw new InvalidOperationException($"Expected {TweenToSymbolName} to have generic type arguments");
        }
        
        var targetComponentTypeSymbol = typeArgumentSymbols[0];
        var valueTypeSymbol = typeArgumentSymbols[1];
        
        var shortName = componentSymbol.Name;
        var longName = componentSymbol.GetFullName();
        var componentNamespace = componentSymbol.ContainingNamespace.ToFullName();

        var fromComponentName = $"{shortName}From";
        var targetComponentName = targetComponentTypeSymbol.ToFullName();
        var valueTypeName = valueTypeSymbol.ToFullName();

        var burstCompileAttribute = targetComponentTypeSymbol.TypeKind == TypeKind.Struct ? "[BurstCompile]" : "";
        
        yield return $$"""
                       using {{componentNamespace}};
                       using Unity.Entities;
                       using HyperTween.ECS.Update.Components;
                       
                       namespace HyperTween.Auto.Components
                       {
                           [global::System.Runtime.CompilerServices.CompilerGenerated]
                           public struct {{fromComponentName}} : IComponentData, ITweenFrom<{{valueTypeName}}>
                           {
                               public {{valueTypeName}} Value;
                               
                               public {{valueTypeName}} GetValue()
                               {
                                   return Value;
                               }
                           
                               public void SetValue({{valueTypeName}} value)
                               {
                                   Value = value;
                               }
                           }
                       }
                       """;
        
        yield return $$"""
                       using {{componentNamespace}};
                       using HyperTween.Auto.Components;
                       using Unity.Entities;
                       using Unity.Burst;
                       using HyperTween.ECS.Structural.Systems;
                       using Unity.Collections;
                       using HyperTween.ECS.Structural.Components;
                       
                       namespace HyperTween.Auto.Systems
                       {
                           [global::System.Runtime.CompilerServices.CompilerGenerated]
                           [UpdateInGroup(typeof(OnTweenStopSystemGroup))]
                           {{burstCompileAttribute}}
                           public partial struct {{shortName}}_RemoveTweenFromComponentSystem : ISystem
                           {
                               private EntityQuery _query;
                               private EntityQuery _singletonQuery;
                           
                               public void OnCreate(ref SystemState state)
                               {
                                   var genericComponentTypes = new NativeList<ComponentType>(4, Allocator.Temp)
                                   {
                                       ComponentType.ReadOnly(typeof({{fromComponentName}}))
                                   };
                           
                                   _query = state.GetEntityQuery(new EntityQueryBuilder(Allocator.Temp)
                                       .WithAll(ref genericComponentTypes)
                                       .WithAll<TweenPlaying, TweenAllowReuse>()
                                       .WithNone<TweenRequestPlaying>());
                                       
                                    _singletonQuery = state.GetEntityQuery(new EntityQueryBuilder(Allocator.Temp)
                                       .WithAll<CleanTweenStructuralChangeECBSystem.Singleton>()
                                       .WithOptions(EntityQueryOptions.IncludeSystems));
                           
                                   genericComponentTypes.Dispose();
                                   
                                   state.RequireForUpdate(_query);
                                   state.RequireForUpdate(_singletonQuery);
                               }
                           
                               {{burstCompileAttribute}}
                               public void OnUpdate(ref SystemState state)
                               {
                                   var ecbSingleton = _singletonQuery.GetSingleton<CleanTweenStructuralChangeECBSystem.Singleton>();
                                   var ecb = ecbSingleton.CreateCommandBuffer(state.WorldUnmanaged);
                           
                                   ecb.RemoveComponent<{{fromComponentName}}>(_query, EntityQueryCaptureMode.AtPlayback);
                               }
                           }
                       }
                       """;

        if (targetComponentTypeSymbol.TypeKind == TypeKind.Struct)
        {
            yield return $$"""
                           using {{componentNamespace}};
                           using HyperTween.Auto.Components;
                           using Unity.Burst;
                           using Unity.Burst.Intrinsics;
                           using Unity.Collections;
                           using Unity.Entities;
                           using HyperTween.ECS.Structural.Components;
                           using HyperTween.ECS.Structural.Systems;
                           using HyperTween.ECS.Update.Components;
                           using HyperTween.ECS.Update.Systems;

                           namespace HyperTween.Auto.Systems
                           {
                               [global::System.Runtime.CompilerServices.CompilerGenerated]
                               [BurstCompile]
                                [UpdateInGroup(typeof(AddTweenFromSystemGroup))]
                               public partial struct {{shortName}}_AddTweenFromComponentSystem : ISystem
                               {
                                   private EntityTypeHandle _entityTypeHandle;
                                   private ComponentTypeHandle<{{targetComponentName}}> _targetComponentHandle;
                                   private ComponentTypeHandle<TweenTarget> _tweenTargetHandle;
                                   private ComponentLookup<{{targetComponentName}}> _targetComponentLookup;
                                   
                                   private EntityQuery _singletonQuery, _query;
                               
                                   public void OnCreate(ref SystemState state)
                                   {
                                       _entityTypeHandle = state.GetEntityTypeHandle();
                                       _targetComponentHandle = state.GetComponentTypeHandle<{{targetComponentName}}>(true);
                                       _tweenTargetHandle = state.GetComponentTypeHandle<TweenTarget>(true);
                                       _targetComponentLookup = state.GetComponentLookup<{{targetComponentName}}>();
                                       
                                       _singletonQuery = state.GetEntityQuery(new EntityQueryBuilder(Allocator.Temp)
                                           .WithAll<AddTweenFromECBSystem.Singleton>()
                                           .WithOptions(EntityQueryOptions.IncludeSystems));
                                       
                                       _query = state.GetEntityQuery(new EntityQueryBuilder(Allocator.Temp)
                                           .WithAll<{{shortName}}>()
                                           .WithAny<TweenPlaying, TweenForceOutput>()
                                           .WithNone<{{fromComponentName}}>());
                               
                                       state.RequireForUpdate(_query);        
                                       state.RequireForUpdate(_singletonQuery);        
                                   }
                               
                                   [BurstCompile]
                                   public void OnUpdate(ref SystemState state)
                                   {
                                       _entityTypeHandle.Update(ref state);
                                       _targetComponentHandle.Update(ref state);
                                       _tweenTargetHandle.Update(ref state);
                                       _targetComponentLookup.Update(ref state);
                                       
                                       var ecbSystem = _singletonQuery.GetSingleton<AddTweenFromECBSystem.Singleton>();
                                       var ecb = ecbSystem.CreateCommandBuffer(state.WorldUnmanaged);
                                       
                                       state.Dependency = new AddTweenFromJob<{{fromComponentName}}, {{shortName}}, {{targetComponentName}}, {{valueTypeName}}>()
                                       {
                                           TargetComponentHandle = _targetComponentHandle,
                                           EntityTypeHandle = _entityTypeHandle,
                                           EntityCommandBuffer = ecb.AsParallelWriter(),
                                           TweenTargetComponentHandle = _tweenTargetHandle,
                                           TargetComponents = _targetComponentLookup
                                       }.ScheduleParallel(_query, state.Dependency);
                                   }
                               }
                           }
                           """;

            yield return $$"""
                           using {{componentNamespace}};
                           using HyperTween.Auto.Components;
                           using HyperTween.ECS.Structural.Components;
                           using HyperTween.ECS.Update.Components;
                           using Unity.Burst;
                           using Unity.Collections;
                           using Unity.Entities;
                           using HyperTween.ECS.Update.Systems;

                           namespace HyperTween.Auto.Systems
                           {
                               [global::System.Runtime.CompilerServices.CompilerGenerated]
                               [BurstCompile]
                               [UpdateInGroup(typeof(TweenOutputSystemGroup))]
                               public partial struct {{shortName}}_WriteTweenOutputSystem : ISystem
                               {
                                   private ComponentTypeHandle<{{fromComponentName}}> _tweenFromHandle;
                                   private ComponentTypeHandle<{{longName}}> _tweenToHandle;
                                   private ComponentTypeHandle<TweenParameter> _tweenParameterHandle;
                                   private ComponentTypeHandle<{{targetComponentName}}> _targetComponentHandle;
                                   private ComponentTypeHandle<TweenTarget> _tweenTargetHandle;
                                   private ComponentLookup<{{targetComponentName}}> _targetComponents;
                                   
                                   private EntityQuery _query;
                               
                                   private void OnCreate(ref SystemState state)
                                   {
                                       _tweenFromHandle = state.GetComponentTypeHandle<{{fromComponentName}}>(true);
                                       _tweenToHandle = state.GetComponentTypeHandle<{{longName}}>(true);
                                       _tweenParameterHandle = state.GetComponentTypeHandle<TweenParameter>(true);
                                       _targetComponentHandle = state.GetComponentTypeHandle<{{targetComponentName}}>(false);
                                       _tweenTargetHandle = state.GetComponentTypeHandle<TweenTarget>(true);
                                       _targetComponents = state.GetComponentLookup<{{targetComponentName}}>(false);
                           
                                       _query = state.GetEntityQuery(new EntityQueryBuilder(Allocator.Temp)
                                           .WithAll<{{fromComponentName}}, {{longName}}, {{targetComponentName}}, TweenParameter>()
                                            .WithAny<TweenPlaying, TweenForceOutput>());
                                           
                                       state.RequireAnyForUpdate(_query);
                                   }
                               
                                   [BurstCompile]
                                   private void OnUpdate(ref SystemState state)
                                   {
                                       _tweenFromHandle.Update(ref state);
                                       _tweenToHandle.Update(ref state);
                                       _targetComponentHandle.Update(ref state);
                                       _tweenTargetHandle.Update(ref state);
                                       _tweenParameterHandle.Update(ref state);
                                       _targetComponents.Update(ref state);
                                       
                                       state.Dependency = new WriteTweenOutputJob<{{fromComponentName}}, {{longName}}, {{targetComponentName}}, {{valueTypeName}}>()
                                       {
                                           TargetComponentHandle = _targetComponentHandle,
                                           TweenFromHandle = _tweenFromHandle,
                                           TweenParameterHandle = _tweenParameterHandle,
                                           TweenToHandle = _tweenToHandle,
                                           TargetComponents = _targetComponents,
                                           TweenTargetHandle = _tweenTargetHandle
                                       }.ScheduleParallel(_query, state.Dependency);
                                   }
                               }
                           }
                           """;
        }
        else if (targetComponentTypeSymbol.TypeKind == TypeKind.Class)
        {
            yield return $$"""
                           using {{componentNamespace}};
                           using HyperTween.Auto.Components;
                           using Unity.Burst;
                           using Unity.Burst.Intrinsics;
                           using Unity.Collections;
                           using Unity.Entities;
                           using HyperTween.ECS.Structural.Components;
                           using HyperTween.ECS.Structural.Systems;
                           using HyperTween.ECS.Update.Components;
                           using HyperTween.ECS.Update.Systems;

                           namespace HyperTween.Auto.Systems
                           {
                               [global::System.Runtime.CompilerServices.CompilerGenerated]
                               [UpdateInGroup(typeof(OnTweenPlaySystemGroup))]
                               public partial struct {{shortName}}_AddTweenFromComponentSystem : ISystem
                               {
                                   private EntityTypeHandle _entityTypeHandle;
                                   private ComponentTypeHandle<{{targetComponentName}}> _targetComponentHandle;

                                   private EntityQuery _singletonQuery, _query;
                               
                                   public void OnCreate(ref SystemState state)
                                   {
                                       _entityTypeHandle = state.GetEntityTypeHandle();
                                       _targetComponentHandle = state.EntityManager.GetComponentTypeHandle<{{targetComponentName}}>(false);

                                       _singletonQuery = state.GetEntityQuery(new EntityQueryBuilder(Allocator.Temp)
                                           .WithAll<CleanTweenStructuralChangeECBSystem.Singleton>()
                                           .WithOptions(EntityQueryOptions.IncludeSystems));
                                       
                                       _query = state.GetEntityQuery(new EntityQueryBuilder(Allocator.Temp)
                                           .WithAll<{{shortName}}, {{targetComponentName}}, TweenRequestPlaying>()
                                           .WithNone<TweenPlaying, {{fromComponentName}}>());
                               
                                       state.RequireForUpdate(_query);        
                                       state.RequireForUpdate(_singletonQuery);        
                                   }
                               
                                   public void OnUpdate(ref SystemState state)
                                   {
                                       _entityTypeHandle.Update(ref state);
                                       _targetComponentHandle.Update(ref state);

                                       var ecbSystem = _singletonQuery.GetSingleton<CleanTweenStructuralChangeECBSystem.Singleton>();
                                       var ecb = ecbSystem.CreateCommandBuffer(state.WorldUnmanaged);
                                       
                                       using var chunks = _query.ToArchetypeChunkArray(Allocator.Temp);
                                       foreach (var chunk in chunks)
                                       {
                                           var targetComponents = chunk.GetManagedComponentAccessor<{{targetComponentName}}>(ref _targetComponentHandle, state.EntityManager);
                                           var entities = chunk.GetNativeArray(_entityTypeHandle);
                                           
                                           var enumerator = new ChunkEntityEnumerator(false, default, chunk.Count);
                                           while(enumerator.NextEntityIndex(out var i))
                                           {
                                               var targetComponent = targetComponents[i];
                                               var from = default({{shortName}}).Read(in targetComponent);
                                               
                                               var tweenFrom = default({{fromComponentName}});
                                               tweenFrom.SetValue(from);
                                               
                                               ecb.AddComponent(entities[i], tweenFrom);
                                           }
                                       }
                                   }
                               }
                           }
                           """;

            yield return $$"""
                           using {{componentNamespace}};
                           using HyperTween.Auto.Components;
                           using HyperTween.ECS.Structural.Components;
                           using HyperTween.ECS.Update.Components;
                           using Unity.Burst;
                           using Unity.Collections;
                           using Unity.Entities;
                           using HyperTween.ECS.Update.Systems;

                           namespace HyperTween.Auto.Systems
                           {
                               [global::System.Runtime.CompilerServices.CompilerGenerated]
                               [UpdateInGroup(typeof(TweenOutputSystemGroup))]
                               public partial struct {{shortName}}_WriteTweenOutputSystem : ISystem
                               {
                                   private ComponentTypeHandle<{{fromComponentName}}> _tweenFromHandle;
                                   private ComponentTypeHandle<{{longName}}> _tweenToHandle;
                                   private ComponentTypeHandle<TweenParameter> _tweenParameterHandle;
                                   private ComponentTypeHandle<{{targetComponentName}}> _targetComponentHandle;
                                   
                                   private EntityQuery _query;
                               
                                   private void OnCreate(ref SystemState state)
                                   {
                                       _tweenFromHandle = state.GetComponentTypeHandle<{{fromComponentName}}>(true);
                                       _tweenToHandle = state.GetComponentTypeHandle<{{longName}}>(true);
                                       _tweenParameterHandle = state.GetComponentTypeHandle<TweenParameter>(true);
                                       _targetComponentHandle = state.EntityManager.GetComponentTypeHandle<{{targetComponentName}}>(false);

                                       _query = state.GetEntityQuery(new EntityQueryBuilder(Allocator.Temp)
                                           .WithAll<{{fromComponentName}}, {{longName}}, {{targetComponentName}}, TweenParameter, TweenPlaying>()
                                           .WithNone<TweenTarget>());

                                       state.RequireAnyForUpdate(_query);
                                   }
                               
                                   private void OnUpdate(ref SystemState state)
                                   {
                                       _tweenFromHandle.Update(ref state);
                                       _tweenToHandle.Update(ref state);
                                       _targetComponentHandle.Update(ref state);
                                       _tweenParameterHandle.Update(ref state);
                                       
                                       using var chunks = _query.ToArchetypeChunkArray(Allocator.Temp);
                                       foreach (var chunk in chunks)
                                       {
                                           var tweenFroms = chunk.GetNativeArray(ref _tweenFromHandle);
                                           var tweenTos = chunk.GetNativeArray(ref _tweenToHandle);
                                           var tweenParameters = chunk.GetNativeArray(ref _tweenParameterHandle);
                                           var targetComponents = chunk.GetManagedComponentAccessor<{{targetComponentName}}>(ref _targetComponentHandle, state.EntityManager);
                                                
                                           var enumerator = new ChunkEntityEnumerator(false, default, chunk.Count);
                                           while(enumerator.NextEntityIndex(out var i))
                                           {
                                               var from = tweenFroms[i].GetValue();
                                               var toComponent = tweenTos[i];
                                               var to = toComponent.GetValue();
                                               var interpolated = toComponent.Lerp(from, to, (float)tweenParameters[i].Value);
                                           
                                               var targetComponent = targetComponents[i];
                                               toComponent.Write(ref targetComponent, interpolated);
                                           }
                                       }
                                   }
                               }
                           }
                           """;
        }
        else
        {
            throw new NotSupportedException("ITweenTo only supports structs and classes");
        }
    }
}