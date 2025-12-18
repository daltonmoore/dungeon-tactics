using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace Editor.Tutorials.ListView
{
    public class PlanetsWindow : EditorWindow
    {
        [SerializeField] protected VisualTreeAsset uxmlAsset;

        // Nested interface that can be either a single planet or a group of planets.
        protected interface IPlanetOrGroup
        {
            public string name { get; }

            public bool populated { get; }
        }

        // Nested class that represents a planet.
        protected class Planet : IPlanetOrGroup
        {
            public string name { get; }
            public bool populated { get; }

            public Planet(string name, bool populated)
            {
                this.name = name;
                this.populated = populated;
            }
        }

        // Nested class that represents a group of planets
        protected class PlanetGroup : IPlanetOrGroup
        {
            public string name { get; }

            public bool populated
            {
                get
                {
                    var anyPlanetPopulated = false;
                    foreach (Planet planet in planets)
                    {
                        anyPlanetPopulated = anyPlanetPopulated || planet.populated;
                    }

                    return anyPlanetPopulated;
                }
            }

            public readonly IReadOnlyList<Planet> planets;

            public PlanetGroup(string name, IReadOnlyList<Planet> planets)
            {
                this.name = name;
                this.planets = planets;
            }
        }

        // Data about planets in our solar system.
        protected static readonly List<PlanetGroup> planetsGroups = new()
        {
            new PlanetGroup("Inner Planets", new List<Planet>
            {
                new Planet("Mercury", false),
                new Planet("Venus", false),
                new Planet("Earth", false),
                new Planet("Mars", false),
            }),
            new PlanetGroup("Outer Planets", new List<Planet>
            {
                new Planet("Jupiter", false),
                new Planet("Saturn", false),
                new Planet("Uranus", false),
                new Planet("Neptune", false),
            }),
        };

        // Expresses planet data as a list of the planets themselves. Needed for ListView and MultiColumnView.
        protected static List<Planet> units
        {
            get
            {
                var retVal = new List<Planet>(8);
                foreach (PlanetGroup group in planetsGroups)
                {
                    retVal.AddRange(group.planets);
                }
                
                return retVal;
            }
        }
        
        // Expresses planet data as a list of TreeViewItemData objects. Needed for TreeView and MultiColumnTreeView
        protected static IList<TreeViewItemData<IPlanetOrGroup>> treeRoots
        {
            get
            {
                int id = 0;
                var roots = new List<TreeViewItemData<IPlanetOrGroup>>(planetsGroups.Count);
                foreach (PlanetGroup group in planetsGroups)
                {
                    var planetsInGroup = new List<TreeViewItemData<IPlanetOrGroup>>(group.planets.Count);
                    foreach (var planet in group.planets) 
                    {
                        planetsInGroup.Add(new TreeViewItemData<IPlanetOrGroup>(id++, planet));
                    }
                    
                    roots.Add(new TreeViewItemData<IPlanetOrGroup>(id++, group, planetsInGroup));
                }
                return roots;
            }
        }
    }
}
