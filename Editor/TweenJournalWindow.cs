using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using HyperTween.ECS.Update.Components;
using Unity.Collections;
using Unity.Entities;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;

namespace HyperTween.TweenDebug.Journal
{
    public static class Singleton<T>
    {
        private static T _instance;

        public static T Instance => _instance ??= ConstructionFunc();
        public static Func<T> ConstructionFunc { get; set; }
        public static bool HasInstance => _instance != null;

        public static bool TryGetInstance(out T instance)
        {
            instance = _instance;
            return _instance != null;
        }
    }

    /// <summary>
    /// Detect world list changes when new worlds are created or old worlds are destroyed.
    /// </summary>
    struct WorldListChangeTracker
    {
        int m_Count;
        ulong m_CurrentHash;

        /// <returns>If called after World(s) have been created or destroyed, will return true *once*.</returns>
        public bool HasChanged()
        {
            var newCount = World.All.Count;
            ulong newHash = 0;
            foreach (var world in World.All)
                newHash += world.SequenceNumber;

            var hasChanged = m_Count != newCount || newHash != m_CurrentHash;
            m_CurrentHash = newHash;
            m_Count = newCount;
            return hasChanged;
        }
    }

    public static class WorldNamesCache
    {
        private static WorldListChangeTracker _worldListChangeTracker;
        private static string[] _names;

        public static string[] Names => (!_worldListChangeTracker.HasChanged() ? _names : _names = GetNames()) ?? Array.Empty<string>();

        private static string[] GetNames()
        {
            var names = new string[World.All.Count];
            for (var i = 0; i < World.All.Count; i++)
            {
                var world = World.All[i];
                names[i] = world.Name;
            }
            return names;
        }
    }
    
    public class TweenJournalWindow : EditorWindow
    {
        private EntityQuery _singletonQuery;
        private Vector2 _scrollPos;

        private World _world;
        public World World
        {
            get => _world;
            set
            {
                _world = value;
                
                if (_world == null)
                {
                    _singletonQuery = default;
                    return;
                }
                
                _singletonQuery = new EntityQueryBuilder(Allocator.Temp)
                    .WithAll<TweenJournalSingleton>()
                    .Build(value.EntityManager);
            }
        }

        private int _selectedWorldIndex = -1;
        private bool _isCreated;
        
        [MenuItem("Window/HyperTween/Journal")]
        static void OpenWindow() => GetWindow<TweenJournalWindow>();

        private readonly Dictionary<Entity, string> _entityNameCache = new(512);
        private readonly Dictionary<Entity, string> _entityToStringCache = new(512);
        private readonly Dictionary<Entity, double> _durationCache = new(512);
        
        private void OnEnable()
        {
            _isCreated = true;
        }

        private void OnGUI()
        {
            if (!_isCreated)
            {
                return;
            }
            
            if (World.All.Count == 0)
            {
                return;
            }
            
            var newSelectedIndex = EditorGUILayout.Popup(_selectedWorldIndex, WorldNamesCache.Names);
            if (newSelectedIndex != _selectedWorldIndex || _world == null)
            {
                _selectedWorldIndex = newSelectedIndex;
                if (_selectedWorldIndex >= 0 && _selectedWorldIndex < World.All.Count)
                {
                    World = World.All[_selectedWorldIndex];
                }
                else
                {
                    World = null;
                }
            }

            if (_world is not { IsCreated: true })
            {
                return;
            }

            _singletonQuery.CompleteDependency();

            if (!_singletonQuery.TryGetSingleton<TweenJournalSingleton>(out var singleton))
            {
                return;
            }
            
            var columnLayoutOptions = new []
            {
                GUILayout.Width(100), GUILayout.ExpandWidth(false)
            };
            
            _scrollPos = EditorGUILayout.BeginScrollView(_scrollPos);
            EditorGUILayout.BeginHorizontal();
                        
            EditorGUILayout.BeginVertical(columnLayoutOptions);
            EditorGUILayout.LabelField("Index", columnLayoutOptions);
            foreach (var entry in singleton.Buffer)
            {
                if (entry.LiteEntry.Entity == Entity.Null)
                {
                    continue;
                }
                
                EditorGUILayout.LabelField(entry.Index.ToString(CultureInfo.InvariantCulture), columnLayoutOptions);
            }
            EditorGUILayout.EndVertical();
            
            EditorGUILayout.BeginVertical(columnLayoutOptions);
            EditorGUILayout.LabelField("Entity", columnLayoutOptions);
            foreach (var entry in singleton.Buffer)
            {
                if (entry.LiteEntry.Entity == Entity.Null)
                {
                    continue;
                }

                if (!_entityToStringCache.TryGetValue(entry.LiteEntry.Entity, out var entityToString))
                {
                    entityToString = entry.LiteEntry.Entity.ToString();
                    _entityToStringCache[entry.LiteEntry.Entity] = entityToString;
                }
                EditorGUILayout.LabelField(entityToString, columnLayoutOptions);
            }
            EditorGUILayout.EndVertical();
            
            
            
            EditorGUILayout.BeginVertical(columnLayoutOptions);
            EditorGUILayout.LabelField("Name", columnLayoutOptions);
            foreach (var entry in singleton.Buffer)
            {
                if (entry.LiteEntry.Entity == Entity.Null)
                {
                    continue;
                }

                if (!_entityNameCache.TryGetValue(entry.LiteEntry.Entity, out var entityName))
                {
                    entityName = _world.EntityManager.GetName(entry.LiteEntry.Entity);
                    _entityNameCache[entry.LiteEntry.Entity] = entityName;
                }
                EditorGUILayout.LabelField(entityName, columnLayoutOptions);
            }
            EditorGUILayout.EndVertical();
            
            EditorGUILayout.BeginVertical(columnLayoutOptions);
            EditorGUILayout.LabelField("Event", columnLayoutOptions);
            foreach (var entry in singleton.Buffer)
            {
                if (entry.LiteEntry.Entity == Entity.Null)
                {
                    continue;
                }

                EditorGUILayout.LabelField(entry.LiteEntry.Event.ToString(), columnLayoutOptions);
            }
            EditorGUILayout.EndVertical();
            
            EditorGUILayout.BeginVertical(columnLayoutOptions);
            EditorGUILayout.LabelField("Frame", columnLayoutOptions);
            foreach (var entry in singleton.Buffer)
            {
                if (entry.LiteEntry.Entity == Entity.Null)
                {
                    continue;
                }

                EditorGUILayout.LabelField(entry.Frame.ToString(), columnLayoutOptions);
            }
            EditorGUILayout.EndVertical();
            
            EditorGUILayout.BeginVertical(columnLayoutOptions);
            EditorGUILayout.LabelField("Iteration", columnLayoutOptions);
            foreach (var entry in singleton.Buffer)
            {
                if (entry.LiteEntry.Entity == Entity.Null)
                {
                    continue;
                }
                
                EditorGUILayout.LabelField(entry.Iteration.ToString(), columnLayoutOptions);
            }
            EditorGUILayout.EndVertical();
            
            EditorGUILayout.BeginVertical(columnLayoutOptions);
            EditorGUILayout.LabelField("Time", columnLayoutOptions);
            foreach (var entry in singleton.Buffer)
            {
                if (entry.LiteEntry.Entity == Entity.Null)
                {
                    continue;
                }

                EditorGUILayout.LabelField(entry.Time.ToString(CultureInfo.InvariantCulture), columnLayoutOptions);
            }
            EditorGUILayout.EndVertical();
            
            EditorGUILayout.BeginVertical(columnLayoutOptions);
            EditorGUILayout.LabelField("Duration", columnLayoutOptions);
            foreach (var entry in singleton.Buffer)
            {
                if (entry.LiteEntry.Entity == Entity.Null)
                {
                    continue;
                }

                var entity = entry.LiteEntry.Entity;
                double duration = -1;

                if (entry.LiteEntry.Event == TweenJournal.Event.Stop)
                {
                    if (!_durationCache.TryGetValue(entity, out duration))
                    {

                        foreach (var e in singleton.Buffer)
                        {
                            if (e.LiteEntry.Event != TweenJournal.Event.Play || !e.LiteEntry.Entity.Equals(entity))
                            {
                                continue;
                            }

                            duration = entry.Time - e.Time;
                            _durationCache.Add(entity, duration);
                            break;
                        }
                    }
                }
                EditorGUILayout.LabelField(duration.ToString(CultureInfo.InvariantCulture), columnLayoutOptions);
            }
            EditorGUILayout.EndVertical();
            
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.EndScrollView();
            
            Repaint();
        }
    }

}