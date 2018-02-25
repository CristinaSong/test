
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace 计算每个站点间的差距
{

    /// <summary>
    /// 权重
    /// </summary>
    public class Option
    {
        private static double _w1;

        public static double W1
        {
            get { return Option._w1; }
            set { Option._w1 = value; }
        }

        private static double _w2;

        public static double W2
        {
            get { return Option._w2; }
            set { Option._w2 = value; }
        }

    }
    /// <summary>
    /// 站点表
    /// </summary>
    public class Station
    {
        private int _id;
        public int Id
        {
            get { return _id; }
            set { _id = value; }
        }

        private double _lng;
        public double Lng
        {
            get { return _lng; }
            set { _lng = value; }
        }

        private double _lat;
        public double Lat
        {
            get { return _lat; }
            set { _lat = value; }
        }
        /// <summary>
        /// 读站点文件
        /// </summary>
        /// <param name="str1"></param>
        /// <returns></returns>
        public static Dictionary<int,Station> sta_Read(string str1)
        {
            //读文件
            StreamReader sr = new StreamReader(new FileStream(str1, FileMode.Open));
            //获取文件内容
            Dictionary<int, Station> dicS = sr.ReadToEnd().Split(new[] { "\n" }, StringSplitOptions.RemoveEmptyEntries).ToList().Select(line =>
            {
                string[] arr = line.Split(new[] { "\t" }, StringSplitOptions.RemoveEmptyEntries);
                return new Station
                {
                    Id = int.Parse(arr[0]),
                    Lng = double.Parse(arr[1]),
                    Lat = double.Parse(arr[2])
                };
            }).ToDictionary(Key => Key.Id, Value => Value);//Dictionary的Id为Key，Value为Station
            return dicS;
        }
    }
    /// <summary>
    /// 地域表
    /// </summary>
    public class Zone
    {
        private int _zoneId;//记录地域标识码
        public int ZoneId
        {
            get { return _zoneId; }
            set { _zoneId = value; }
        }

        private List<int> _staNos;//组成该地域站点标识码
        public List<int> StaNos
        {
            get { return _staNos; }
            set { _staNos = value; }
        }
        public int StaNum//组成该地域站点数目
        {
            get
            {
                return _staNos.Count; //只读
            }

        }
        private int _inD;//入度
        public int InD
        {
            get { return _inD; }
            set { _inD = value; }
        }

        private int _outD;//出度
        public int OutD
        {
            get { return _outD; }
            set { _outD = value; }
        }
        public Zone(){
            this.StaNos = new List<int>();//将staNos初始化为List
    }
        /// <summary>
        /// 初始化地域
        /// </summary>
        /// <param name="dicS"></param>
        /// <param name="dicSta"></param>
        /// <returns></returns>
        public static bool zone_Ini(Dictionary<int, Station> dicS, out Dictionary<int, Zone> dicSta)
        {
            dicSta = new Dictionary<int, Zone>();//使用out引用需要初始化
            foreach (var item in dicS)
            {
                Zone zone = new Zone();
                zone.ZoneId = item.Key;//地域ID从1开始
                zone.StaNos.Add(item.Value.Id);//组成该地域站点标识码添加到地域的StaNos
                dicSta.Add(zone.ZoneId, zone);
            }
            return true;
        }
        /// <summary>
        /// 判断两个地域是否相邻
        /// </summary>
        /// <param name="z1"></param>
        /// <param name="z2"></param>
        /// <param name="zadjvalue"></param>
        /// <returns></returns>
        public static bool zone_dis(Zone z1, Zone z2, double zadjvalue)
        {
            if (z1.ZoneId == z2.ZoneId) return true;//同一个区域
            double adj = 0, att = 0, tadj = 0;
            List<double> att1 = new List<double> { 0, 0, 0, 0, 0 };
            List<double> att2 = new List<double> { 0, 0, 0, 0, 0 };
            int num1 = z1.StaNos.Count();
            int num2 = z2.StaNos.Count();
            int n = 0;
            for (int i = 0; i <num1; i++)
                for (int j = 0; j <num2; j++)
                    if ((tadj = Data.adj_value[z1.StaNos[i]-1][z2.StaNos[j]-1]) > 0)
                    {
                        adj += tadj;
                        n++;
                    }
            if (n != 0) adj /= n;//临界值加起来求平均值

            if (adj == 0) return false;
            for (int i = 0; i <num1; i++)
                for (int k = 0; k < 5; k++)
                    att1[k] += Data.pnt_atttable[z1.StaNos[i]-1][k];

            for (int j = 0; j < num2; j++)
                for (int k = 0; k < 5; k++)
                    att2[k] += Data.pnt_atttable[z2.StaNos[j] - 1][k];


            for (int k = 0; k < 5; k++)
            {
                att += Math.Abs(att1[k] / num1 - att2[k] / num2);
            }
            adj /= att;
            return adj > zadjvalue;//定义两个地域zi，zj是相邻的，当且仅当ADJ（zi,zj）>=r=2成立，否则不相邻。r为地域合并最小值，只有相邻的地域才可能合并。

            //return true;
        }

    }

    /// <summary>
    ///搭乘移动记录属性表
    /// </summary>
    public class MoveRcd
    {
        private int _moveId;

        public int MoveId
        {
            get { return _moveId; }
            set { _moveId = value; }
        }
        private int _sId;

        public int SId
        {
            get { return _sId; }
            set { _sId = value; }
        }
        private int eId;

        public int EId
        {
            get { return eId; }
            set { eId = value; }
        }
        private int i_MoveNum;

        public int I_MoveNum
        {
            get { return i_MoveNum; }
            set { i_MoveNum = value; }
        }
        private int _moveNum;

        public int MoveNum
        {
            get { return _moveNum; }
            set { _moveNum = value; }
        }
        /// <summary>
        /// 读轨迹文件
        /// </summary>
        /// <param name="str2"></param>
        /// <returns></returns>
        public static Dictionary <int,MoveRcd> move_Read(string str2)
        {

            StreamReader s = new StreamReader(new FileStream(str2, FileMode.Open));
            Dictionary<int, MoveRcd> dicMov = s.ReadToEnd().Split(new[] { "\n" }, StringSplitOptions.RemoveEmptyEntries).ToList().Select(line =>
            {
                string[] arr = line.Split(new[] { "\t" }, StringSplitOptions.RemoveEmptyEntries);
                return new MoveRcd
                {
                    MoveId = int.Parse(arr[0]),
                    SId = int.Parse(arr[1]),
                    EId = int.Parse(arr[2]),
                    I_MoveNum = int.Parse(arr[3]),
                    MoveNum = int.Parse(arr[4])
                };
            }).ToDictionary(Key=>Key.MoveId,Value=>Value);
            s.Close();//文件打开必须关闭
            return dicMov;
        }
        /// <summary>
        /// 计算轨迹阈值为100的情况
        /// </summary>
        /// </summary>
        /// <param name="path"></param>
        /// <param name="min"></param>
        /// <returns></returns>
        public static int movenum = 0;
        public static List<MoveRcd> move_Ini(string path, int min)
        {
            //过滤掉移动数目小于100的移动
            //思路：读取全部的txt文件，然后按照txt中数据的规则将数据截取到数组中，然后筛选最后一列(保留>=100的轨迹）
            Dictionary<int,MoveRcd> mlist = MoveRcd.move_Read(path);
            List<MoveRcd> nlist = new List<MoveRcd>();
            mlist.Select(x=>x.Value).ToList().ForEach(item =>
            {
                if (item.MoveNum >= min)
                {
                    nlist.Add(new MoveRcd
                    {
                        MoveId = item.MoveId,
                        
                        SId = item.SId,
                        EId = item.EId,
                        I_MoveNum = item.I_MoveNum,
                        MoveNum = item.MoveNum
                    });
                    movenum += item.MoveNum;
                }
            }
                );
            StreamWriter sw = new StreamWriter(new FileStream("../../../move_statistic.txt", FileMode.OpenOrCreate));
            int mId = 0;
            nlist.ForEach(x =>
            {
                //sw.WriteLine(x.MoveId + " " + x.RcdId + " " + x.SId + " " + x.EId + " " + x.MoveNum);
                sw.WriteLine(mId + " " + x.SId + " " + x.EId + " " + x.MoveNum);
                mId++;
            });
            return nlist;
        }


    }
    /// <summary>
    /// 移动模式字段属性
    /// </summary>
    public class MPZ
    {
        private int _patternId;

        public int PatternId
        {
            get { return _patternId; }
            set { _patternId = value; }
        }
        private int _sId;

        public int SId
        {
            get { return _sId; }
            set { _sId = value; }
        }
        private int _eId;

        public int EId
        {
            get { return _eId; }
            set { _eId = value; }
        }
        private int _moveNum;

        public int MoveNum
        {
            get { return _moveNum; }
            set { _moveNum = value; }
        }
        private List<int> _moveNos;

        public List<int> MoveNos
        {
            get { return _moveNos; }
            set { _moveNos = value; }
        }
        
        private double _v;
        public double V
        {
            get { return _v; }
            set { _v = value; }
        }
        private double _a;

        public double A
        {
            get { return _a; }
            set { _a = value; }
        }
        private double _c;

        public double C
        {
            get { return _c; }
            set { _c = value; }
        }
        public MPZ()
        {
            this.MoveNos = new List<int>();
        }
        /// <summary>
        /// 初始化轨迹
        /// </summary>
        /// <param name="str2"></param>
        /// <returns></returns>
        public static bool move_Ini(Dictionary<int, MoveRcd> dicMov, out Dictionary<int, MPZ> dicMpz,int min)//要将dicMpz传出去，所以用out
        {
            dicMpz = new Dictionary<int, MPZ>();//初始化
            int i = 0;
            foreach (var item in dicMov)//item是dicMov的每一条数据
            {
                if (item.Value.MoveNum>=min)
                {
                    MPZ mpz = new MPZ();
                    //mpz.PatternId = item.Value.MoveId;
                    mpz.PatternId=i++;
                    mpz.SId = item.Value.SId;
                    mpz.EId = item.Value.EId;
                    mpz.MoveNos.Add(item.Value.MoveId);//该搭乘记录所属的标识码的集合
                    mpz.MoveNum = item.Value.MoveNum;//移动模式包含移动记录的数量
                    dicMpz.Add(mpz.PatternId, mpz);
                }
            }
            return true;
        }
    }
}
