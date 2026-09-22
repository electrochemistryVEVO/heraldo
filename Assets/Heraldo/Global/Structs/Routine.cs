using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Heraldo.Global.Structs
{
    public struct Routine
    {
        public Vector3[] NodeList;
        public bool Loopable;

        public Routine(Vector3[] nodeList, bool loopable)
        {
            NodeList = nodeList;
            Loopable = loopable;
        }

        public static Routine FromGameObject(GameObject obj)
        {
            Transform[] steps = obj.GetComponentsInChildren<Transform>();
            Vector3[] _nodeList = steps.ToList()
                .Where((val) => val != obj.transform)
                .Select((val) => val.position)
                .ToArray();
            return new Routine(_nodeList,true);
        }

        public int Length()
        {
            return NodeList.Length;
        }
    }
}