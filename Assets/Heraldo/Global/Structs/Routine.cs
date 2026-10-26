using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Heraldo.Global.Structs
{
    public struct Routine
    {
        public Vector3[] NodeList;
        public bool Loopable;
        //Determines whether routine should be followed regardless of other encounters 
        public bool Hurry;

        public Routine(Vector3[] nodeList, bool loopable, bool hurry = true)
        {
            NodeList = nodeList;
            Loopable = loopable;
            Hurry = hurry;
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