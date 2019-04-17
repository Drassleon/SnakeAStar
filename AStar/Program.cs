using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Windows;


namespace WannaBeSnake
{
    class Program
    {
        public struct Node
        {

            public int g;
            public int h;
            public int f;
            public int x;
            public int y;
            public int xfather;
            public int yfather;
            public Node(int xNode, int yNode, int heuristic, int realDistance)
            {
                h = heuristic;
                g = realDistance + 10;
                f = g + h;
                x = xNode;
                y = yNode;
                xfather = 0;
                yfather = 0;
            }
            public void IncreaseG()
            {
                g += 10;
            }
            public void SetFather(int xF,int yF)
            {
                xfather = xF;
                yfather = yF;
            }
            public void UpdateDistance()
            {
                f = g + h;
            }
        }
        //Manhattan Distance
        static public int Manhattan(int x1, int y1, int x2, int y2)
        {
            return (Math.Abs(x2 - x1) + Math.Abs(y2 - y1)) * 10;
        }
        static public void AStar(int x1, int y1, int x2, int y2,List<Position> path,List<Position> bodyPositions)
        {
            path.Clear();
            int[,] map = new int[30, 150];
            
            List<Node> open = new List<Node>();
            List<Node> closed = new List<Node>();
            Node node1 = new Node(x1 + 1, y1, Manhattan(x1 + 1, y1, x2, y2), 0);
            Node node2 = new Node(x1, y1 + 1, Manhattan(x1, y1 + 1, x2, y2), 0);
            Node node3 = new Node(x1 - 1, y1, Manhattan(x1 - 1, y1, x2, y2), 0);
            Node node4 = new Node(x1, y1 - 1, Manhattan(x1, y1 - 1, x2, y2), 0);
            foreach(Position bodyPart in bodyPositions)
            {
                map[bodyPart.Y, bodyPart.X] = -1;
            }
            open.Add(node1);
            open.Add(node2);
            open.Add(node3);
            open.Add(node4);


            bool solutionFound = false;
          
            while (!solutionFound)
            {
                if (open.Count==0)
                {
                    break;
                }
                open = open.OrderBy(Node => Node.f).ToList();

                closed.Add(open[0]);
                if (open[0].x == x2 && open[0].y == y2)
                {
                    solutionFound = true;
                    break;
                }

                node1 = new Node(open[0].x + 1, open[0].y, Manhattan(open[0].x + 1, open[0].y, x2, y2), open[0].g);
                node2 = new Node(open[0].x, open[0].y + 1, Manhattan(open[0].x + 1, open[0].y + 1, x2, y2), open[0].g);
                node3 = new Node(open[0].x - 1, open[0].y, Manhattan(open[0].x - 1, open[0].y, x2, y2), open[0].g);
                node4 = new Node(open[0].x, open[0].y - 1, Manhattan(open[0].x, open[0].y - 1, x2, y2), open[0].g);
                node1.SetFather(open[0].x, open[0].y);
                node2.SetFather(open[0].x, open[0].y);
                node3.SetFather(open[0].x, open[0].y);
                node4.SetFather(open[0].x, open[0].y);

                List<Node> nodesToAdd = new List<Node>();
                List<Node> nodesToRemove = new List<Node>();

                nodesToAdd.Add(node1);
                nodesToAdd.Add(node2);
                nodesToAdd.Add(node3);
                nodesToAdd.Add(node4);
                for (int i = 0; i < open.Count; i++)
                {
                    for (int j = 0; j < nodesToAdd.Count; j++)
                    {
                        if (open[i].x == nodesToAdd[j].x && open[i].y == nodesToAdd[j].y)
                        {
                                //open[i].IncreaseG();
                                nodesToRemove.Add(nodesToAdd[j]);
                        }
                    }
                }

                foreach (Node node in nodesToAdd)
                {
                    foreach (Node nodes in closed)
                    {
                        if ((node.x == nodes.x && node.y == nodes.y)||node.x<=-1||node.y<=-1||node.y>=30||node.x>=150||map[node.y,node.x]==-1)
                        {
                            nodesToRemove.Add(node);
                            continue;
                        }
                    }
                }
                foreach (Node node in nodesToRemove)
                {
                    nodesToAdd.Remove(node);
                }
                foreach (Node node in nodesToAdd)
                {
                    if(map[node.y, node.x] != -1)
                    open.Add(node);
                }
                open.Remove(open[0]);
            }
            /*foreach (Node node in closed)
            {
                path.Add(new Position(node.x, node.y));
            }*/
            Node auxNode = closed.Last();
            path.Add(new Position(auxNode.x, auxNode.y));
            while (true)
            {
                if(auxNode.xfather==0&&auxNode.yfather==0)
                {
                    path.Add(new Position(auxNode.x, auxNode.y));
                    break;
                }
                path.Add(new Position(auxNode.xfather,auxNode.yfather));
                auxNode = closed.Find(x => x.x == path.Last().X && x.y == path.Last().Y);
            }
            path.Reverse();
        }
        public struct Position
        {
            public int X, Y;
            public Position(int x, int y)
            {
                X = x;
                Y = y;
            }
        }

        public struct Manzanita
        {
            public int X, Y;
            public Manzanita(int x, int y)
            {
                X = x;
                Y = y;
            }

            public void Respawn()
            {
                Random rnd = new Random();
                X = rnd.Next(2, 148);
                Y = rnd.Next(0, 28);
            }

            public void Draw()
            {
                Console.SetCursorPosition(X, Y);
                Console.BackgroundColor = ConsoleColor.Black;
                Console.Write("*");
            }
        }

        public struct Snake
        {
            public int highscore;

            public int X, Y, dX, dY, bodyLen, steps, appleX, appleY;
            public List<Position> bodyPositions;
            public List<Position> smartPath;
            public bool bodyFull, grow, isSmart, newGoal;

            public Snake(int x, int y)
            {
                highscore = 0;
                X = x;
                Y = y;
                dX = 1;
                dY = 0;
                bodyLen = 1;
                bodyFull = false;
                grow = false;
                bodyPositions = new List<Position>();
                smartPath = new List<Position>();
                /*smartPath.Add(new Position(11, 10));
                smartPath.Add(new Position(12, 10));
                smartPath.Add(new Position(13, 10));
                smartPath.Add(new Position(13, 11));
                smartPath.Add(new Position(13, 12));
                smartPath.Add(new Position(13, 13));
                smartPath.Add(new Position(14, 13));
                smartPath.Add(new Position(15, 13));
                smartPath.Add(new Position(15, 14));
                smartPath.Add(new Position(15, 15));*/
                appleX = 0;
                appleY = 0;
                newGoal = true;
                isSmart = false;
                steps = 0;
            }

            public void loseMessage()
            {
                //MessageBox.Show("PERDISTE CON UN PUNTAJE TOTAL DE -> " + (bodyLen - 1), "Aceptar");
                reborn();
            }

            public void reborn()
            {
                X = 10;
                Y = 10;
                dX = 1;
                dY = 0;
                bodyLen = 1;
                bodyFull = false;
                grow = false;
                bodyPositions = new List<Position>();
                Console.Clear();
            }

            public void redirect(ConsoleKey myKey)
            {

                switch (myKey)
                {
                    case ConsoleKey.W:
                        if (dY != 1)
                        {
                            dY = -1;
                            dX = 0;
                        }
                        break;
                    case ConsoleKey.S:
                        if (dY != -1)
                        {
                            dY = 1;
                            dX = 0;
                        }
                        break;
                    case ConsoleKey.A:
                        if (dX != 1)
                        {
                            dY = 0;
                            dX = -1;
                        }
                        break;
                    case ConsoleKey.D:
                        if (dX != -1)
                        {
                            dY = 0;
                            dX = 1;
                        }
                        break;
                }

            }

            public int distance(Position p1, Position p2)
            {
                return Math.Abs(p1.X - p2.X) + Math.Abs(p1.Y - p2.Y);
            }




            public void moveSnake()
            {
                if (isSmart)
                {
                    /*
                    if (smartPath[steps].X > X)
                    {
                        dX = 1;
                        dY = 0;
                    }
                    if (smartPath[steps].X < X)
                    {
                        dX = -1;
                        dY = 0;
                    }
                    if (smartPath[steps].Y > Y)
                    {
                        dY = 1;
                        dX = 0;
                    }
                    if (smartPath[steps].Y < Y)
                    {
                        dY = -1;
                        dX = 0;
                    }*/
                    /*foreach(Position pos in bodyPositions)
                    {
                        if(pos.X==smartPath[0].X&&pos.Y==smartPath[0].Y)
                        {
                            drawSnake();
                        }
                    }
                    X = smartPath[0].X;
                    Y = smartPath[0].Y;*/
                    if (steps >= smartPath.Count)
                    {
                        if(bodyLen>highscore)
                        {
                            highscore = bodyLen;
                        }
                        
                        Thread.Sleep(10000);
                        
                        loseMessage();
                        AStar(X, Y, appleX, appleY, smartPath, bodyPositions);
                        steps = 0;
                    }
                    else
                    {
                        X = smartPath[steps].X;
                        Y = smartPath[steps].Y;
                        if (smartPath[steps].X == appleX && smartPath[steps].Y == appleY)
                        {
                            steps = 0;
                        }
                        steps++;
                    }
                }
                else
                {
                    X += dX;
                    Y += dY;
                }



                /*for (int i = 0; i < bodyPositions.Count; i++)
                {
                    if (X == bodyPositions[i].X && Y == bodyPositions[i].Y)
                    {
                        loseMessage();
                        break;
                    }
                }*/
                if (X < 0) X = 149;
                if (Y < 0) Y = 29;
                if (X > 149) X = 0;
                if (Y > 29) Y = 0;

                drawSnake();
            }

            public void growUp()
            {
                bodyLen++;
                grow = true;
            }

            public void drawSnake()
            {
                if (!bodyFull)
                {
                    for (int i = 0; i < bodyLen; i++)
                    {
                        Console.SetCursorPosition(X, Y);
                        Console.BackgroundColor = ConsoleColor.White;
                        Console.Write(" ");
                        bodyPositions.Add(new Position(X, Y));
                        X++;
                    }
                    bodyFull = true;
                }
                else
                {
                    Console.SetCursorPosition(bodyPositions[0].X, bodyPositions[0].Y);
                    Console.BackgroundColor = ConsoleColor.Black;
                    Console.Write(" ");
                    bodyPositions.RemoveAt(0);
                    Console.SetCursorPosition(X, Y);
                    Console.BackgroundColor = ConsoleColor.White;
                    Console.Write(" ");
                    bodyPositions.Add(new Position(X, Y));
                    if (grow)
                    {
                        X += dX;
                        Y += dY;
                        if (X < 1) X = 149;
                        if (Y < 1) Y = 29;
                        if (X > 149) X = 0;
                        if (Y > 29) Y = 0;
                        Console.SetCursorPosition(X, Y);
                        Console.BackgroundColor = ConsoleColor.White;
                        Console.Write("*");
                        bodyPositions.Add(new Position(X, Y));
                        grow = false;
                    }
                }

            }
        }




        static void Main(string[] args)
        {
            Random rnd = new Random();
            Manzanita myApple = new Manzanita(20, 20);
            Console.CursorVisible = false;
            Console.SetWindowSize(150, 30);
            Snake mySnake = new Snake(10, 10)
            {
                isSmart = true,
                appleX = myApple.X,
                appleY = myApple.Y
            };
            AStar(mySnake.X, mySnake.Y, mySnake.appleX, mySnake.appleY, mySnake.smartPath,mySnake.bodyPositions);

            var keyPressEvent = Task.Run(() => mySnake.redirect(Console.ReadKey(true).Key));
            ConsoleKey mykey = new ConsoleKey();

            var keyHold = Task.Run(() => mykey = Console.ReadKey(true).Key);
            while (true)
            {
                Console.SetCursorPosition(1, 0);
                Console.Title="MANZANITAS COMIDAS: " + (mySnake.bodyLen - 1)+"| HighScore:"+mySnake.highscore;
                //Thread.Sleep(1);
                if (mySnake.X == myApple.X && mySnake.Y == myApple.Y)
                {
                    //Console.Beep(200, 200);
                    mySnake.growUp();
                    myApple.Respawn();
                    CheckApple:
                    foreach(Position pos in mySnake.bodyPositions)
                    {
                        if(myApple.X==pos.X&&myApple.Y==pos.Y)
                        {
                            
                            myApple.Respawn();
                            goto CheckApple;
                        }
                    }
                    mySnake.appleX = myApple.X;
                    mySnake.appleY = myApple.Y;
                    mySnake.newGoal = true;
                    AStar(mySnake.X, mySnake.Y, myApple.X, myApple.Y, mySnake.smartPath, mySnake.bodyPositions);
                }
                //AStar(mySnake.X, mySnake.Y, myApple.X, myApple.Y, mySnake.smartPath, mySnake.bodyPositions);
                mySnake.moveSnake();
                myApple.Draw();
                if (keyPressEvent.IsCompleted) keyPressEvent = Task.Run(() => mySnake.redirect(Console.ReadKey(true).Key));

            }
        }

        public void ColorConsole(ConsoleColor color)
        {
            for (int i = 0; i < 150; i++)
            {
                for (int j = 0; j < 30; j++)
                {
                    Console.SetCursorPosition(i, j);
                    Console.BackgroundColor = color;
                }
            }
            Console.SetCursorPosition(0, 0);
        }

    }

}



