using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Unity.Entities.SourceGen.Common;

namespace HyperTweenGenerators;

[Generator]
public class DetectConflictsSystemGenerator : ISourceGenerator
{
    private const string TweenToSymbolName = "HyperTween.ECS.Update.Components.ITweenTo";
    private const string AutoDetectConflictsSymbolName = "HyperTween.ECS.ConflictDetection.Attributes.DetectConflictsAttribute";

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
            
            var autoDetectConflictsStructs = new List<(ITypeSymbol,ITypeSymbol?)>();

            foreach (var syntaxTree in syntaxTrees)
            {
                var semanticModel = compilation.GetSemanticModel(syntaxTree);
                
                var structDeclarationSyntaxes = syntaxTree.GetRoot()
                    .DescendantNodes()
                    .OfType<StructDeclarationSyntax>();

                foreach (var structDeclaration in structDeclarationSyntaxes)
                {
                    var structSymbol = semanticModel.GetDeclaredSymbol(structDeclaration) as ITypeSymbol;

                    if (structSymbol == null)
                    {
                        continue;
                    }

                    if (!structSymbol.ImplementsInterface(TweenToSymbolName))
                    {
                        continue;
                    }

                    var autoDetectConflictsAttributeData = structSymbol.GetAttributeData(AutoDetectConflictsSymbolName);
                    if (autoDetectConflictsAttributeData == null)
                    {
                        continue;
                    }
                    
                    var instanceIdComponentTypedConstant = autoDetectConflictsAttributeData.ConstructorArguments
                        .FirstOrDefault();
                    
                    // TODO: Validate that instanceIdComponentType only contains an int, and is an IComponent
                    
                    autoDetectConflictsStructs.Add((structSymbol, instanceIdComponentTypedConstant.Kind != TypedConstantKind.Error ? instanceIdComponentTypedConstant.GetExpressedType() : default(ITypeSymbol)));
                }
            }

            var generatedPartials = GenerateDetectConflictsHelper(autoDetectConflictsStructs);
            
            foreach (var tuple in generatedPartials)
            {
                var (hint, source) = tuple;
                context.AddSource($"{hint}.g.cs", source);
                
                SourceOutputHelpers.OutputSourceToFile(
                    $"Temp/GeneratedCode/{context.Compilation.Assembly.Name}/{hint}.g.cs",
                    () => source);
            }

            stopwatch.Stop();

            SourceOutputHelpers.LogInfoToSourceGenLog($"TIME : {nameof(DetectConflictsSystemGenerator)} : {context.Compilation.Assembly.Name} : {stopwatch.ElapsedMilliseconds}ms");
        }
        catch (Exception exception)
        {
            if (exception is OperationCanceledException)
                throw;

            context.LogError("SGICE002", nameof(DetectConflictsSystemGenerator), exception.ToUnityPrintableString(), lastLocation ?? context.Compilation.SyntaxTrees.First().GetRoot().GetLocation());
        }
    }

    private IEnumerable<(string,string)> GenerateDetectConflictsHelper(List<(ITypeSymbol, ITypeSymbol?)> conflictTypeSymbols)
    {
        var componentNames = conflictTypeSymbols
            .Select(tuple => (tuple.Item1.ToFullName(), tuple.Item2?.ToFullName()))
            .ToArray();

        if (componentNames.Length == 0)
        {
            yield break;
        }

        var dynamicTypeInfoDefinitions = string.Join("\n", componentNames
            .Select( (_,i) => $"[ReadOnly] public ConflictTypeTuple ConflictTypeTuple{i};")
            .ToArray());
        
        var initDynamicTypeInfos = string.Join("\n", componentNames
            .Select( (tuple,i) =>
            {
                if (tuple.Item2 != null)
                {
                    return $"ConflictTypeTuple{i}.Initialise<{tuple.Item1}, {tuple.Item2}>(ref state);";
                }

                return $"ConflictTypeTuple{i}.Initialise<{tuple.Item1}>(ref state);";
            })
            .ToArray());

        var updateDynamicTypeInfos = string.Join("\n", componentNames
            .Select( (_,i) => $"ConflictTypeTuple{i}.Update(ref state);")
            .ToArray());
        
        var componentTypes = string.Join(",\n", componentNames
            .Select(tuple => $"ComponentType.ReadOnly(typeof({tuple.Item1}))")
            .ToArray());

        yield return ("DetectConflictsHelper",$$"""
                       using Unity.Entities;
                       using Unity.Collections;
                       
                       namespace HyperTween.Auto.Systems
                       {
                           [global::System.Runtime.CompilerServices.CompilerGenerated]
                           public class DetectConflictsHelper
                           {
                               public static NativeList<ComponentType> GetConflictComponentTypes()
                               {
                                    return new NativeList<ComponentType>({{componentNames.Length}}, Allocator.Persistent)
                                    {
                                        {{componentTypes}}
                                    };
                               }
                           }
                        }
                       """);

        yield return ("DetectConflictsJobData",$$"""
                     using Unity.Entities;
                     using Unity.Collections;
                     
                     namespace HyperTween.Auto.Systems
                     {
                         public struct DetectConflictsJobData
                         {
                            public struct ConflictTypeTuple
                            {
                                [ReadOnly]
                                public HyperTween.ECS.Util.DynamicTypeInfo TargetComponentTypeInfo;
                                [ReadOnly]
                                public HyperTween.ECS.Util.DynamicTypeInfo InstanceIdComponentTypeInfo;
                                
                                public bool HasInstanceIdComponent;
                                
                                public void Initialise<TTarget>(ref SystemState state)
                                {
                                     TargetComponentTypeInfo.InitialiseReadOnly<TTarget>(ref state);
                                     HasInstanceIdComponent = false;
                                }
                                 
                                public void Initialise<TTarget, TInstanceId>(ref SystemState state)
                                {
                                    TargetComponentTypeInfo.InitialiseReadOnly<TTarget>(ref state);
                                    InstanceIdComponentTypeInfo.InitialiseReadOnly<TInstanceId>(ref state);
                                    HasInstanceIdComponent = true;
                                }
                                
                                public void Update(ref SystemState state)
                                {
                                    TargetComponentTypeInfo.Update(ref state);
                                    if(HasInstanceIdComponent)
                                    {
                                        InstanceIdComponentTypeInfo.Update(ref state);
                                    }
                                }
                            }
                         
                            {{dynamicTypeInfoDefinitions}}

                            public void Initialise(ref SystemState state)
                            {
                                {{initDynamicTypeInfos}}
                            }
                            
                            public void Update(ref SystemState state)
                             {
                                 {{updateDynamicTypeInfos}}
                             }
                         }
                     }
                     """);
    }
}