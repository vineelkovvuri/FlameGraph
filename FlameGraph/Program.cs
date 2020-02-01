using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace FlameGraph
{
    class TreeNode
    {
        public Line Line { get; set; }
        public List<TreeNode> Children { get; set; }
        public override string ToString()
        {
            return $"{Line} {Children.Count}";
        }
    }
    class Line
    {
        public string Function { get; set; }
        public long Begin { get; set; }
        public long End { get; set; }
        public long XBegin { get; set; }
        public long XEnd { get; set; }
        public long Width { get; set; }
        public string Color { get; set; }
        public override string ToString()
        {
            return $"{Function}\n Begin: {Begin}\n End: {End}";
        }
    }
    class Program
    {
        static long factor = 1000;
        static Random random = new Random();
        static void Main(string[] args)
        {
            string input = @"C:\Users\vineelko\Desktop\sample.txt";
            List<Line> list = new List<Line>();
            foreach(string line in File.ReadAllLines(input))
            {
                list.Add(parseLine(line));
            }

            long min = list.Min(x => x.Begin);

            for (int i = 0; i < list.Count; i++)
            {
                list[i].Begin -= min;
                list[i].XBegin = list[i].Begin/factor;
                list[i].End -= min;
                list[i].XEnd = list[i].End/factor;
                list[i].Width = list[i].XEnd - list[i].XBegin;

            }

            list.Sort((line1, line2) =>
            {
                if ((int)(line1.Begin - line2.Begin) == 0)
                {
                    return (int)(line1.End - line2.End);
                }
                return (int)(line1.Begin - line2.Begin);
            });


            string[] colors = new string[]
            {
                "#F55D3E", "#878E88", "#F7CB15", "#76BED0", "#FFD07B", "#1789FC", "#E9DF00", "#01FDF6", "#488286", "#ADA8B6", "#FFEEDB"
            }.OrderBy(x => random.Next()).ToArray();
            Dictionary<string, string> funccolor = new Dictionary<string, string>();
            int k = 0;
            foreach (var entry in list)
            {
                if (!funccolor.ContainsKey(entry.Function))
                {
                    funccolor.Add(entry.Function, colors[k % colors.Length]);
                    k++;
                }
            }

            foreach(var entry in list)
            {
                entry.Color = funccolor[entry.Function];
            }

            List<TreeNode> root = new List<TreeNode> { };

            foreach (var entry in list)
            {
                FlameGraph(root, entry);
            }

            StreamWriter file = new StreamWriter(@"D:\temp\flame.html");
            file.WriteLine("<html> <head> <style> .svgsize {width: 2000000px; } </style> </head> <body> <figure class=\"svgsize\">");
            file.WriteLine("<svg xmlns=\"http://www.w3.org/2000/svg\" width=\"100%\" height=\"100%\">");
            file.WriteLine("<style xmlns=\"http://www.w3.org/2000/svg\" type=\"text/css\">");
            file.WriteLine("    .onhover:hover { stroke:black; stroke-width:0.5; }");
            file.WriteLine("</style>");
            GenerateFlameGraphSVG(root, file, 10);
            file.WriteLine("</svg>");
            file.WriteLine("</figure> </body> </html>");
            file.Close();

        }

        private static void GenerateFlameGraphSVG(List<TreeNode> root, StreamWriter file, int level)
        {
            foreach (TreeNode p in root)
            {
                Line line = p.Line;
                file.WriteLine($"<g class=\"onhover\">");
                file.WriteLine($"<rect x=\"{line.XBegin}\" y=\"{level}\" width=\"{line.Width}\" height=\"30\" style=\"fill:{line.Color};\"/>");
                file.WriteLine($"<text xmlns=\"http://www.w3.org/2000/svg\" text-anchor=\"\" x=\"{line.XBegin + 10}\" y=\"{level + 20}\"  font-size=\"11\" font-family=\"Verdana\" >{line.Function}</text>");
                file.WriteLine($"<title>{line}</title>");
                file.WriteLine($"</g>");
                GenerateFlameGraphSVG(p.Children, file, level + 30);
            }
        }

        static void FlameGraph(List<TreeNode> root, Line line)
        {
            bool asChild = false;
            foreach (TreeNode p in root)
            {
                if (p.Line.Begin <= line.Begin && line.End <= p.Line.End)
                {
                    FlameGraph(p.Children, line);
                    asChild = true;
                    break;
                }
            }

            if (!asChild)
            {
                root.Add(new TreeNode { Line = line, Children = new List<TreeNode>() });
            }
        }

        static Line parseLine(string line)
        {
            int space = line.IndexOf(" ");
            int colon = line.IndexOf(":");
            string func = line.Substring(space + 1, colon - space - 1).Trim();
            int bindex = line.IndexOf("[");
            int eindex = line.IndexOf("]");
            string[] range = line.Substring(bindex + 1, eindex - bindex - 1).Split();
            string begin = range[0];
            string end = range[1];
            return new Line {
                Function = func,
                Begin = long.Parse(begin),
                End = long.Parse(end),
            };
        }
    }
}
