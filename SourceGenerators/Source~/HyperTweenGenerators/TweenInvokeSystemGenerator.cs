using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Unity.Entities.SourceGen.Common;

namespace HyperTweenGenerators;

[Generator]
public class TweenInvokeSystemGenerator : ISourceGenerator
{
    private const string InvokeOnPlaySymbolName = "HyperTween.ECS.Invoke.Components.ITweenInvokeOnPlay";
    private const string InvokeOnStopSymbolName = "HyperTween.ECS.Invoke.Components.ITweenInvokeOnStop";

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

                foreach (var symbol in allSymbols)
                {
                    var generated = new List<string>();
                    var prefix = symbol.TypeKind == TypeKind.Class ? "Managed" : "Unmanaged";
                    
                    if (symbol.TryGetImplementedInterface(InvokeOnPlaySymbolName, out _))
                    {
                        generated.AddRange(GenerateOutputSystemsAndComponents(symbol,
                            $"{prefix}TweenInvokeOnPlaySystem",
                            "OnTweenPlaySystemGroup",
                            "OnPlay",
                            false));
                    }
                    
                    if (symbol.TryGetImplementedInterface(InvokeOnStopSymbolName, out _))
                    {
                        generated.AddRange(GenerateOutputSystemsAndComponents(symbol,
                                $"{prefix}TweenInvokeOnStopSystem",
                                "OnTweenStopSystemGroup",
                                "OnStop",
                                true));
                    }

                    if (generated.Count == 0)
                    {
                        continue;
                    }

                    if (syntaxTreeToGeneratedSourceMap.TryGetValue(syntaxTree, out var list))
                    {
                        list.AddRange(generated);
                    }
                    else
                    {
                        syntaxTreeToGeneratedSourceMap.Add(syntaxTree, generated);

                    }
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
                    var generatedFile = syntaxTree.GetGeneratedSourceFilePath(context.Compilation.Assembly.Name, nameof(TweenInvokeSystemGenerator), salting: i);
                    var outputSource = TypeCreationHelpers.FixUpLineDirectivesAndOutputSource(generatedFile.FullFilePath, generatedPartial);
                    
                    context.AddSource(generatedFile.FileNameOnly, outputSource);
                    
                    SourceOutputHelpers.OutputSourceToFile(
                        generatedFile.FullFilePath,
                        () => outputSource.ToString());
                }
            }

            stopwatch.Stop();

            SourceOutputHelpers.LogInfoToSourceGenLog($"TIME : {nameof(TweenInvokeSystemGenerator)} : {context.Compilation.Assembly.Name} : {stopwatch.ElapsedMilliseconds}ms");
        }
        catch (Exception exception)
        {
            if (exception is OperationCanceledException)
                throw;

            context.LogError("SGICE002", nameof(TweenInvokeSystemGenerator), exception.ToUnityPrintableString(), lastLocation ?? context.Compilation.SyntaxTrees.First().GetRoot().GetLocation());
        }
    }

    private ImmutableArray<JobExecutionParameter> GetInvokeMethodParameters(ITypeSymbol structSymbol)
    {
        var methodSymbol = GetInvokeMethodSymbol(structSymbol);

        return methodSymbol.Parameters.Select(symbol => new JobExecutionParameter(symbol, structSymbol)).ToImmutableArray();
    }

    private static IMethodSymbol GetInvokeMethodSymbol(ITypeSymbol symbol)
    {
        var members = symbol.GetMembers("Invoke");
        if (members.Length > 1)
        {
            throw new InvalidOperationException($"Must be exactly one member present on {symbol.ToFullName()} with the name Invoke");
        }

        if (members[0] is not IMethodSymbol methodSymbol)
        {
            throw new InvalidOperationException($"Invoke member on {symbol.ToFullName()} is not a method");
        }

        return methodSymbol;
    }

    private IEnumerable<string> GenerateOutputSystemsAndComponents(ITypeSymbol symbol, string baseSystemName, string systemGroupName, string invokeSuffix, bool isStop)
    {
        var componentNamespace = symbol.ContainingNamespace.ToFullName();

        var invokeComponentName =$"{symbol.Name}{invokeSuffix}";
        var invokeMethodParameters = GetInvokeMethodParameters(symbol);

        var usings = string.Join("\n", invokeMethodParameters
            .Zip(Enumerable.Range(0, invokeMethodParameters.Length), (parameter, i) => (parameter, i))
            .Select(tuple => tuple.parameter.GetUsing(tuple.i).Trim())
            .Append($"using {componentNamespace};")
            .Append("using HyperTween.ECS.Update.Components;")
            .Distinct());

        var jobData = string.Join("\n", invokeMethodParameters
            .Zip(Enumerable.Range(0, invokeMethodParameters.Length), (parameter, i) => (parameter, i))
            .Select(tuple => tuple.parameter.GetJobDataDefinition(tuple.i)));

        var queryTypeElements = invokeMethodParameters
            .Zip(Enumerable.Range(0, invokeMethodParameters.Length), (parameter, i) => (parameter, i))
            .Select(tuple => tuple.parameter.GetQueryType(tuple.i))
            .Where(s => s != null)
            .Select(s => s ?? string.Empty)
            .Distinct()
            .ToArray();

        var query = queryTypeElements.Length > 0 ?
            $"entityQueryBuilder.WithAll<{string.Join(", ", queryTypeElements)}>();" 
            : string.Empty;
        
        var getNativeArrays = string.Join("\n", invokeMethodParameters
            .Zip(Enumerable.Range(0, invokeMethodParameters.Length), (parameter, i) => (parameter, i))
            .Select(tuple => tuple.parameter.GetNativeArray(tuple.i)));
        
        var reads = string.Join("\n", invokeMethodParameters
            .Zip(Enumerable.Range(0, invokeMethodParameters.Length), (parameter, i) => (parameter, i))
            .Select(tuple => tuple.parameter.GetRead(tuple.i)));

        var componentUsings = string.Join("\n", symbol.DeclaringSyntaxReferences.SelectMany(reference => reference
                .GetSyntax()
                .SyntaxTree
                .GetRoot()
                .DescendantNodes()
                .OfType<UsingDirectiveSyntax>())
            .Select(syntax => syntax.ToFullString().Trim())
            // Because the generated component will have a different namespace
            .Append($"using {symbol.ContainingNamespace};")
            .Append("using HyperTween.ECS.Update.Components;")
            .Distinct());
        
        var componentMembers =  string.Join("\n", symbol
            .GetMemberSyntax()
            .Select(syntax => syntax.ToFullString()));
        
        var systemName = $"{symbol.Name}_{baseSystemName}";
        
        var triggerQuery = isStop
            ? """
              entityQueryBuilder
              .WithAll<TweenPlaying>()
              .WithNone<TweenRequestPlaying>();
              """
            : """
              entityQueryBuilder
              .WithAll<TweenRequestPlaying>()
              .WithNone<TweenPlaying>();
              """;
                
        var initialiseJobDatas = string.Join("\n", invokeMethodParameters
            .Zip(Enumerable.Range(0, invokeMethodParameters.Length), (parameter, i) => (parameter, i))
            .Select(tuple => tuple.parameter.GetInitialiseJobData(tuple.i)));
        
        var updateJobDatas = string.Join("\n", invokeMethodParameters
            .Zip(Enumerable.Range(0, invokeMethodParameters.Length), (parameter, i) => (parameter, i))
            .Select(tuple => tuple.parameter.GetUpdateJobData(tuple.i)));
        
        var invokeParams = string.Join(", ", invokeMethodParameters
            .Zip(Enumerable.Range(0, invokeMethodParameters.Length), (parameter, i) => (parameter, i))
            .Select(tuple => tuple.parameter.GetJobInvokeParam(tuple.i)));
        
        var writes = string.Join("\n", invokeMethodParameters
            .Zip(Enumerable.Range(0, invokeMethodParameters.Length), (parameter, i) => (parameter, i))
            .Select(tuple => tuple.parameter.GetWrite(tuple.i)));

        if (symbol.TypeKind == TypeKind.Struct)
        {

            var allowParallel = invokeMethodParameters.All(parameter => parameter.AllowParallel());
            var schedule = allowParallel ? 
                "state.Dependency = job.ScheduleParallel(_query, state.Dependency);" :
                "state.Dependency = job.Schedule(_query, state.Dependency);";


            yield return $$"""
                           {{componentUsings}}
                           using Unity.Burst;
                           
                           namespace HyperTween.Auto.Components
                           {
                               [global::System.Runtime.CompilerServices.CompilerGenerated]
                               [BurstCompile]
                               public struct {{symbol.Name}}{{invokeSuffix}} : Unity.Entities.IComponentData
                               {
                                   {{componentMembers}}
                               }
                           }
                           """;
            
            yield return $$"""
                           using Unity.Entities;
                           using Unity.Burst;
                           using Unity.Burst.Intrinsics;
                           using HyperTween.ECS.Structural.Components;
                           using HyperTween.ECS.Structural.Systems;
                           using HyperTween.ECS.Invoke.Systems;
                           using Unity.Collections;
                           using HyperTween.Auto.Components;
                           {{usings}}

                           namespace HyperTween.Auto.Systems
                           {
                               [global::System.Runtime.CompilerServices.CompilerGenerated]
                               [UpdateInGroup(typeof({{systemGroupName}}))]
                               [BurstCompile]
                               public partial struct {{systemName}} : ISystem
                               {
                                   public struct InvokeJobData
                                   {
                                       {{jobData}}
                                       
                                       public EntityTypeHandle EntityTypeHandle;
                                       [ReadOnly]
                                       public ComponentTypeHandle<TweenTarget> TweenTargetTypeHandle;
                           
                                    // TODO: Determine if can use ReadOnly here
                                        public ComponentTypeHandle<{{invokeComponentName}}> InvokeTypeHandle;
                                   }
                                   
                                   [BurstCompile]
                                   public struct InvokeJob : ITweenInvokeJob<InvokeJobData>
                                   {
                                       public InvokeJobData JobData { get; set; }
                               
                                       [BurstCompile]
                                       public void Execute(in ArchetypeChunk chunk, int unfilteredChunkIndex, bool useEnabledMask, in v128 chunkEnabledMask)
                                       {
                                           var jobData = JobData;
                               
                                           {{getNativeArrays}}
                                           var invokeComponents = chunk.GetNativeArray(ref jobData.InvokeTypeHandle);
                                           var entities = chunk.GetNativeArray(jobData.EntityTypeHandle);
                           
                                           if (chunk.Has<TweenTarget>())
                                           {
                                               var tweenTargetComponents = chunk.GetNativeArray(ref jobData.TweenTargetTypeHandle);
                                               var enumerator = new ChunkEntityEnumerator(useEnabledMask, chunkEnabledMask, chunk.Count);
                                               while(enumerator.NextEntityIndex(out var i))
                                               {
                                                   {{reads}}
                                                   var tweenEntity = entities[i];
                                                   var targetEntity = tweenTargetComponents[i].Target;
                                                   var invokeComponent = invokeComponents[i];
                                               
                                                   invokeComponent.Invoke({{invokeParams}});
                                                   
                                                   {{writes}}
                                                   // TODO: Only perform this write if we can detect that Invoke() writes to a member
                                                   // Also need to account for child method calls...
                                                   invokeComponents[i] = invokeComponent;
                                               }
                                           }
                                           else
                                           {
                                               var enumerator = new ChunkEntityEnumerator(useEnabledMask, chunkEnabledMask, chunk.Count);
                                               while(enumerator.NextEntityIndex(out var i))
                                               {
                                                   {{reads}}
                                                   var tweenEntity = entities[i];
                                                   var targetEntity = tweenEntity;
                                                   var invokeComponent = invokeComponents[i];
                           
                                                   invokeComponent.Invoke({{invokeParams}});
                                                   
                                                   {{writes}}
                                                   // TODO: Only perform this write if we can detect that Invoke() writes to a member
                                                   // Also need to account for child method calls...
                                                   invokeComponents[i] = invokeComponent;
                                               }
                                           }
                                           
                                       }
                               
                                   }
                                    
                                   private EntityQuery _query, _singletonQuery;
                                   private InvokeJobData _jobData;
                                   
                                   private void OnCreate(ref SystemState state)
                                   {
                                       var entityQueryBuilder = new EntityQueryBuilder(Allocator.Temp)
                                           .WithAll<{{invokeComponentName}}>();
                                       
                                       {{triggerQuery}}
                                       {{query}}
                           
                                       _query = entityQueryBuilder.Build(ref state);
                                   
                                       _singletonQuery = state.GetEntityQuery(new EntityQueryBuilder(Allocator.Temp)
                                           .WithAll<TweenStructuralChangeECBSystem.Singleton>()
                                           .WithOptions(EntityQueryOptions.IncludeSystems));
                           
                                       {{initialiseJobDatas}}
                                       _jobData.EntityTypeHandle = state.GetEntityTypeHandle();
                                       _jobData.TweenTargetTypeHandle = state.GetComponentTypeHandle<TweenTarget>();
                                       _jobData.InvokeTypeHandle = state.GetComponentTypeHandle<{{invokeComponentName}}>();
                                       
                                       state.RequireForUpdate(_query);
                                   }
                                   
                                   [BurstCompile]
                                   void ISystem.OnUpdate(ref SystemState state)
                                   {
                                       var ecbSingleton = _singletonQuery.GetSingleton<TweenStructuralChangeECBSystem.Singleton>();
                                       var ecb = ecbSingleton.CreateCommandBuffer(state.WorldUnmanaged);
                           
                                       var job = default(InvokeJob);
                                       
                                       {{updateJobDatas}}
                                       _jobData.EntityTypeHandle.Update(ref state);
                                       _jobData.TweenTargetTypeHandle.Update(ref state);
                                       _jobData.InvokeTypeHandle.Update(ref state);
                           
                                       job.JobData = _jobData;
                                       
                                       {{schedule}}            
                                   }
                               }
                           }
                           """;
        } 
        else if (symbol.TypeKind == TypeKind.Class)
        {
            yield return $$"""
                           {{componentUsings}}
                           using Unity.Burst;

                           namespace HyperTween.Auto.Components
                           {
                               [global::System.Runtime.CompilerServices.CompilerGenerated]
                               public class {{symbol.Name}}{{invokeSuffix}} : Unity.Entities.IComponentData
                               {
                                   {{componentMembers}}
                               }
                           }
                           """;
            
            yield return $$"""
                           using Unity.Entities;
                           using Unity.Burst;
                           using Unity.Burst.Intrinsics;
                           using HyperTween.ECS.Structural.Components;
                           using HyperTween.ECS.Structural.Systems;
                           using HyperTween.ECS.Invoke.Systems;
                           using Unity.Collections;
                           using HyperTween.Auto.Components;
                           {{usings}}

                           namespace HyperTween.Auto.Systems
                           {
                               [global::System.Runtime.CompilerServices.CompilerGenerated]
                               [UpdateInGroup(typeof({{systemGroupName}}))]
                               public partial struct {{systemName}} : ISystem
                               {
                                   public struct InvokeJobData
                                   {
                                       {{jobData}}
                                       
                                       public EntityTypeHandle EntityTypeHandle;
                                       [ReadOnly]
                                       public ComponentTypeHandle<TweenTarget> TweenTargetTypeHandle;
                           
                                        // TODO: Determine if can use ReadOnly here
                                        public ComponentTypeHandle<{{invokeComponentName}}> InvokeTypeHandle;
                                   }
                                   
                                   private EntityQuery _query, _singletonQuery;
                                   private InvokeJobData _jobData;
                                   
                                   private void OnCreate(ref SystemState state)
                                   {
                                       var entityQueryBuilder = new EntityQueryBuilder(Allocator.Temp)
                                           .WithAll<{{invokeComponentName}}>();
                                       
                                       {{triggerQuery}}
                                       {{query}}
                           
                                       _query = entityQueryBuilder.Build(ref state);
                                   
                                       _singletonQuery = state.GetEntityQuery(new EntityQueryBuilder(Allocator.Temp)
                                           .WithAll<TweenStructuralChangeECBSystem.Singleton>()
                                           .WithOptions(EntityQueryOptions.IncludeSystems));
                           
                                       {{initialiseJobDatas}}
                                       _jobData.EntityTypeHandle = state.GetEntityTypeHandle();
                                       _jobData.TweenTargetTypeHandle = state.GetComponentTypeHandle<TweenTarget>();
                                       _jobData.InvokeTypeHandle = state.EntityManager.GetComponentTypeHandle<{{invokeComponentName}}>(false);
                                       
                                       state.RequireForUpdate(_query);
                                   }
                                   
                                   void ISystem.OnUpdate(ref SystemState state)
                                   {
                                       var ecbSingleton = _singletonQuery.GetSingleton<TweenStructuralChangeECBSystem.Singleton>();
                                       var ecb = ecbSingleton.CreateCommandBuffer(state.WorldUnmanaged);

                                       {{updateJobDatas}}
                                       _jobData.EntityTypeHandle.Update(ref state);
                                       _jobData.TweenTargetTypeHandle.Update(ref state);
                                       _jobData.InvokeTypeHandle.Update(ref state);
                           
                                       var jobData = _jobData;
                           
                                       using var chunks = _query.ToArchetypeChunkArray(Allocator.Temp);
                                       foreach (var chunk in chunks)
                                       {
                                           {{getNativeArrays}}
                                           var invokeComponents = chunk.GetManagedComponentAccessor<{{invokeComponentName}}>(ref jobData.InvokeTypeHandle, state.EntityManager);
                                           var entities = chunk.GetNativeArray(jobData.EntityTypeHandle);
                                           
                                           if (chunk.Has<TweenTarget>())
                                           {
                                               var tweenTargetComponents = chunk.GetNativeArray(ref jobData.TweenTargetTypeHandle);
                                               var enumerator = new ChunkEntityEnumerator(false, default, chunk.Count);
                                               while(enumerator.NextEntityIndex(out var i))
                                               {
                                                   {{reads}}
                                                   var tweenEntity = entities[i];
                                                   var targetEntity = tweenTargetComponents[i].Target;
                                                   var invokeComponent = invokeComponents[i];
                                               
                                                   invokeComponent.Invoke({{invokeParams}});
                                                   
                                                   {{writes}}
                                                   // TODO: Only perform this write if we can detect that Invoke() writes to a member
                                                   // Also need to account for child method calls...
                                                   invokeComponents[i] = invokeComponent;
                                               }
                                           }
                                           else
                                           {
                                               var enumerator = new ChunkEntityEnumerator(false, default, chunk.Count);
                                               while(enumerator.NextEntityIndex(out var i))
                                               {
                                                   {{reads}}
                                                   var tweenEntity = entities[i];
                                                   var targetEntity = tweenEntity;
                                                   var invokeComponent = invokeComponents[i];
                                           
                                                   invokeComponent.Invoke({{invokeParams}});
                                                   
                                                   {{writes}}
                                                   // TODO: Only perform this write if we can detect that Invoke() writes to a member
                                                   // Also need to account for child method calls...
                                                   invokeComponents[i] = invokeComponent;
                                               }
                                           }
                                        }
                                   }
                               }
                           }
                           """;
        }
        else
        {
            throw new InvalidOperationException($"Invalid Invoke type: {symbol.TypeKind}");
        }
    }
}