﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ILNumerics;
using NCalc;

namespace TLM.Core
{
    [Serializable]
    public class Net
    {
        public event EventHandler<int> Progress;
        public event EventHandler<string> StatusUpdate;
        public event EventHandler CalcDone;

        public double dL, Ylt, c, f0, x, y, Z0, lambda0, tal0, tc, dT, Vlt, Zlt;
        public int N;
        public int[] shape;
        public List<Node> Nodes;
        public Boundaries boundaries;
        public string Fk;
        public Material material;
        public List<Material> matList;
        

        public Net() { }
        public Net(double sizeX, double sizeY, Material mat, double dL, double Z0, double f0, double c, int N, Boundaries bounds)
        {
            
            ILRetArray<double> vecX = ILNumerics.ILMath.vec<double>(0, dL, sizeX).ToArray();
            ILRetArray<double> vecY = ILNumerics.ILMath.vec<double>(0, dL, sizeY).ToArray();
            double sqrt2 = Math.Sqrt(2.0);

            this.shape = new int[2] { vecX.Count(), vecY.Count() };
            this.dL = dL;
            this.material = mat;
            this.c = c;
            this.f0 = f0;
            this.dL = dL;
            this.N = N;
            this.Z0 = Z0;            
            this.lambda0 = this.c / this.f0;
            this.tal0 = this.lambda0 / this.c;
            this.tc = (5 * this.tal0) / 2;
            this.dT = this.dL / (sqrt2 * this.c);
            this.Vlt = sqrt2 * this.c;
            this.Zlt = sqrt2 * this.Z0;
            this.Ylt = 1 / this.Zlt;
            this.boundaries = bounds;

            Nodes = new List<Node>();
            foreach (var x in vecX)
            {
                int j = vecX.ToList().IndexOf(x);
                foreach (var y in vecY)
                {
                    int i = vecY.ToList().IndexOf(y);
                    bool input = j == 0;
                    Node newNode = new Node(i, j, this.material, this.dL, this.Ylt, this.N, input);
                    Nodes.Add(newNode);
                }
            }
        }

        public Material getMaterial(string name)
        {
            return (from mat in this.matList where mat.Name == name select mat).FirstOrDefault();
        }

        public Node GetNode(int i, int j)
        {
            Node n = (from node in this.Nodes
                      where node.i == i && node.j == j
                      select node).FirstOrDefault();
            return n;
        }

        public void Transmit(Node node, int k)
        {
            node.Vi.P1[k] = node.j < shape[0] - 1 ? GetNode(node.i, node.j + 1).Vr.P3[k - 1] : this.boundaries.Bottom * node.Vr.P1[k - 1];
            node.Vi.P2[k] = node.i > 0 ? GetNode(node.i - 1, node.j).Vr.P4[k - 1] : this.boundaries.Left * node.Vr.P2[k - 1];
            node.Vi.P3[k] = node.j > 0 ? GetNode(node.i, node.j - 1).Vr.P1[k - 1] : this.boundaries.Top * node.Vr.P3[k - 1];
            node.Vi.P4[k] = node.i < shape[1] - 1 ? GetNode(node.i + 1, node.j).Vr.P2[k - 1] : this.boundaries.Right * node.Vr.P4[k - 1];
            node.Vi.P5[k] = node.Vr.P5[k - 1];
        }

        public void Run()
        {
            for (int k = 0; k < this.N - 1; k++)
            {
                //Excitação
                Expression exc = new Expression(Fk);
                exc.Parameters["k"] = k;
                exc.Parameters["dT"] = dT;
                exc.Parameters["f0"] = f0;
                exc.Parameters["Pi"] = Math.PI;
                var inputNodes = (from node in Nodes
                                  where node.input == true
                                  select node).ToList();

                var value = (double)exc.Evaluate();
                Parallel.ForEach(inputNodes, n => n.SetEz(k, value));

                //Solve Scatter
                var needSolve = (from node in Nodes
                                 where node.Vi.NeedToSolve(k)
                                 select node).ToList();
                Parallel.ForEach(needSolve, n => n.SolveScatter(k));
                //Transmit
                Parallel.ForEach(Nodes, n => Transmit(n, k + 1));

                Progress.Invoke(this, k);
#if DEBUG
                Console.Write(String.Format("\rSolving {0} iteration...", k));
#endif
            }
        }


    }
}