using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Splines;

namespace Heraldo.Character.Pathfinding
{

    public class MetaPath
    {
        public double f;
        public double g;
        public MetaPath previous;
        public Path current;

        public MetaPath(double f, double g, MetaPath previous, Path current)
        {
            this.f = f;
            this.g = g;
            this.previous = previous;
            this.current = current;
        }
    }
    public class Path
    { 
        
        public int pos_x;
        public int pos_y;
        public Vector3 realPos;
        public double g;
        public Path next = null;
        public bool arrived;
        public bool dirty;

        public Vector2Int GetPos()
        {
            return new Vector2Int(this.pos_x, this.pos_y);
        }

        public Path(Path path)
        {
            pos_x = path.pos_x;
            pos_y = path.pos_y;
            realPos = path.realPos;
            g = path.g;
            next = path.next;
            arrived = path.arrived;
        }
        
        public Path(int x,int y,Vector3 realPos, double _g, Path _next=null,bool _arrived = false)
        {
            pos_x = x;
            pos_y = y;
            this.realPos = realPos;
            g = _g;
            next = _next;
            arrived = _arrived;
        }
    }

    public class AStarGrid : MonoBehaviour
    {
        static readonly int MAX_RECURSION_STEPS = 500;
        
        public LayerMask WorldMapMask;
        public Vector3 MapSize;
        public Vector2Int MapCellNum;
        private static readonly float NAVMESH_OBSTACLE_PENALTY = 99999.0f;
        private List<Vector3> missList = new();
        private Vector2Int MapRatio {get
            {
                return Vector2Int.RoundToInt(new Vector2(MapSize.x / MapCellNum.x, MapSize.z / MapCellNum.y));
            }
        }

        private Vector3 MapStart
        {
            get
            {
                return new Vector3(transform.position.x - MapSize.x/2 + MapRatio.x/2.0f,0,transform.position.z - MapSize.z/2 + MapRatio.y/2.0f);
            }
        } 
        private Dictionary<Vector2Int, Path> _paths = null;
        private void GetDistances()
        { 
            //To make the process of making the graph more optimal, allocate the memory pool at the start 
            //You can put a loading screen in this step if you want to
            _paths = new Dictionary<Vector2Int, Path>(); 
            Vector2 _mapCellSize = new Vector2(MapRatio.x*1.0f,MapRatio.y*1.0f);
            RaycastHit hit; 
            for (int i=0; i < MapCellNum.x * MapCellNum.y; i++)
            {
                var pos = new Vector3(MapStart.x + _mapCellSize.x * (i/MapCellNum.y),
                    MapStart.y+MapSize.y, MapStart.z + _mapCellSize.y * (i%MapCellNum.y)); 
                bool wasHit = Physics.SphereCast(pos,MathF.Max(_mapCellSize.x,_mapCellSize.y),
                    Vector3.down, out hit, float.MaxValue, WorldMapMask);
                if (wasHit)
                { 
                    Vector2Int intPos = new Vector2Int(i/MapCellNum.y,i%MapCellNum.y);
                    _paths.Add(intPos,new Path(intPos.x,intPos.y,hit.point,
                        hit.collider.gameObject.layer == LayerMask.NameToLayer("NavObstacle") ? NAVMESH_OBSTACLE_PENALTY : 0)); 
                } 
                else missList.Add(pos);
            } 
        }

        private double Distance(Vector2Int pos,Vector2Int goal)
        {
            //Manhattan
            if (pos == goal) return 0;
            Path posPath, goalPath;
            bool posInGrid = _paths.TryGetValue(pos, out posPath);
            bool goalInGrid = _paths.TryGetValue(goal, out goalPath);
            if (!posInGrid || !goalInGrid) throw new Exception("Operation is out of bounds"); 
            //Y distance is biased towards downward slopes (less cost)
            return Math.Abs(goalPath.realPos.x - posPath.realPos.x) + Math.Abs(goalPath.realPos.y - posPath.realPos.y)
                + Math.Abs(goalPath.realPos.z - posPath.realPos.z);
        }
        
        private double GetCost(double g, Vector2Int pos,Vector2Int goal)
        {
            double h = Distance(pos,goal);
            return h + g;
        }
        
        /*private Path ComputePath(List<Path> pathList, int depth,double g,Vector2Int pos,Vector2Int goal)
        {
            Path currentPath;
            bool hasPath = _paths.TryGetValue(pos,out currentPath);
            double origG = 0;
            if (hasPath)
            {
                origG = currentPath.g;
                //currentPath = new Path(currentPath);
                //currentPath.g = g;
                currentPath.next = null;
                currentPath.dirty = true;
                pathList.Add(currentPath);
            }
            else
            {
                Debug.Log("[A-STAR] Dead end: "+pos);
                return null;
            }
            if (depth <= 0)
            {
                return currentPath;
            }
            double minCost = Double.MaxValue; //Penalty for not finding any path
            Path optimalPath = null; 
            for (int i = 0; i < 9; i++)
            {
                if (i / 3 == 1 && i % 3 == 1) continue;
                Vector2Int destination = new Vector2Int(pos.x + (i/3-1),pos.y + (i%3-1));
                double cost = GetCost(g+Distance(pos,destination), destination, goal);
                //Obtener camino de destination
                Path destinationPath;
                hasPath = _paths.TryGetValue(destination, out destinationPath);
                if (hasPath)
                { 
                    //Maximum slope: 45
                    //if (destinationPath.realPos.y - currentPath.realPos.y > MapRatio.x) continue;
                    //if(destinationPath.dirty)continue;
                    cost += destinationPath.g;
                    //destinationPath.g = g + cost;
                    destinationPath.next = null;
                }
                else continue; 
                
                if (destination.x == goal.x && destination.y == goal.y)
                {
                    optimalPath = new Path(destinationPath);
                    optimalPath.arrived = true;
                    currentPath.arrived = true;
                    break; 
                }
                
                if (cost < minCost)
                { 
                    optimalPath = destinationPath;
                    minCost = cost; 
                } 
            }
            //Debug.Log(minCost);
            if (optimalPath != null && !optimalPath.GetPos().Equals(goal))
            {
                Vector2Int nextPos = optimalPath.GetPos();
                optimalPath = ComputePath(pathList,depth - 1, g + Distance(pos,nextPos), nextPos, goal);
            }
            //else optimalPath = new Path(optimalPath);
            currentPath.g = origG;
            currentPath.next = optimalPath;
            //currentPath.dirty = false; 
            return currentPath;
        }
        */

        private void ComputePath(List<Path> pathList, int depth,double g,Vector2Int pos,Vector2Int goal)
        {
            SortedDictionary<int, MetaPath> openMap = new();
            Path begin = _paths[pos];
            double beginCost = GetCost(begin.g, pos, goal);
            int lowestCost = Int32.MaxValue;
            openMap.Add((int)Math.Round(beginCost*100),new MetaPath(beginCost,0,null,begin));
            int k = MAX_RECURSION_STEPS;
            while (openMap.Count > 0 && --k>0)
            {
                var pair = openMap.First();
                openMap.Remove(pair.Key);
                lowestCost = lowestCost < pair.Key ? lowestCost : pair.Key;
                MetaPath current = pair.Value;
                if (current.current.GetPos() == goal)
                {
                    Path curPath = current.current;
                    curPath.arrived = true;
                    while (current.previous is not null)
                    {
                        pathList.Insert(pathList.Count,current.current); 
                        current = current.previous;
                    }
                    return;
                }
                for (int i = 0; i < 9; i++)
                {
                    if (i / 3 == 1 && i % 3 == 1) continue;
                    Path neighbor;
                    bool hasPath = _paths.TryGetValue(current.current.GetPos() + new Vector2Int(i / 3-1, i % 3-1), out neighbor);
                    if(!hasPath)continue;
                    double g_guess = current.g + GetCost(neighbor.g, current.current.GetPos(), neighbor.GetPos());
                    double f_neighbor = g_guess + Distance(neighbor.GetPos(), goal);
                    int i_neighbor = (int)Math.Round(f_neighbor * 100);
                    if (!openMap.ContainsKey(i_neighbor) || openMap[i_neighbor].g > g_guess)
                    {
                        neighbor.dirty = true;
                        MetaPath metaNeighbor = new MetaPath(f_neighbor, g_guess,
                            current, neighbor);
                        openMap[i_neighbor] = metaNeighbor;
                    }
                }
            }
           
        }
        
        void Awake()
        {
            GetDistances();
        } 
        private void OnDrawGizmos()
        {
            Gizmos.DrawWireCube(transform.position,MapSize);
            // foreach (var miss in missList)
            // {
            //     Gizmos.DrawCube(miss,Vector3.one*MapRatio.x);
            // }
        }

        void CompressPath(List<Path> paths)
        {
            for(int i=1;i<paths.Count-1;i++)
            {
                Vector2 dist1 = paths[i + 1].GetPos() - paths[i].GetPos();
                Vector2 dist2 = paths[i].GetPos() - paths[i - 1].GetPos();
                if(((dist1.normalized) - (dist2.normalized)).magnitude < 0.001)
                {
                    paths.RemoveAt(i);
                    i--;
                }
            }
        }

        public async Task<List<Path>> GetPath(Vector3 pos, Vector3 goal)
        {
            if(_paths==null)GetDistances(); //Primer chequeo
            Debug.Log("Primer casilla de grid: ("+MapStart.x+";"+MapStart.z+")");
            //Convert vec3 to grid
            Vector3 roundedPos = pos - MapStart;
            Vector3 roundedGoal = goal - MapStart;
            //Debug.Log("Rounded pos: ("+roundedPos.x+";"+roundedPos.z+")");
            //Debug.Log("Rounded goal: ("+roundedGoal.x+";"+roundedGoal.z+")");
            Vector2Int _pos = new Vector2Int((int)Math.Round(roundedPos.x / MapRatio.x), (int)Math.Round(roundedPos.z / MapRatio.y));
            Vector2Int _goal = new Vector2Int((int)Math.Round(roundedGoal.x / MapRatio.x), (int)Math.Round(roundedGoal.z / MapRatio.y));
            Debug.Log("Pos: ("+_pos.x+";"+_pos.y+")");
            Debug.Log("Goal: ("+_goal.x+";"+_goal.y+")");
            if (_pos == _goal) return null;
            List<Path> pathList = new List<Path>();
            int maxIter = 10;
            do
            {
                List<Path> partialPath = new List<Path>();
                ComputePath(partialPath, MAX_RECURSION_STEPS, 0, _pos, _goal); 
                CompressPath(partialPath);
                pathList.AddRange(partialPath);
            } while (--maxIter>0 && pathList.Count > 0 && !pathList.Last().arrived);
            Debug.Log("[A-STAR] Count of nodes in path: "+pathList.Count);
            return pathList;
        }
    }
}