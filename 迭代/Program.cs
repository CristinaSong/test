using System;
using System.Collections.Generic;
using System.Drawing.Drawing2D;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using 计算每个站点间的差距;
namespace 迭代
{
    class Program
    {
        public static int stop1 = 0;//记录轨迹的条数
        public static int stop2 = -2;
        public static int moveid = 9000;//设大一点，防止ID重复
        public static double[,] matrix = null;
        public static int m_iMinMoveVal = 100;// 最小移动数目
        public static double m_dzonemax = 2;// 地域相邻阈值
        public static double m_dmaxstop = 175;//迭代终止阈值,迭代最大adj值
       // public static double zadjvalue = 10;//地域相邻阈值 
        static void Main(string[] args)
        {
            Dictionary<int, Zone> dicSta;//out内部会初始化，所以外部可以不初始化
            Dictionary<int, MPZ> dicMpz = new Dictionary<int, MPZ>();
            string str1 = "../../../pnt_project.txt";
            string str2 = "../../../move_rcd2.txt";
            Dictionary<int, Station> dicS = Station.sta_Read(str1);//读站点文件
            if (!Zone.zone_Ini(dicS, out dicSta))//将站点初始化为地域得到地域集
            {
                Console.WriteLine("地域初始化失败！");
                return;//异常结束
            }
            Dictionary<int, MoveRcd> dicMov = MoveRcd.move_Read(str2);//读轨迹文件
            if (!MPZ.move_Ini(dicMov, out dicMpz, m_iMinMoveVal))//将移动轨迹初始化为MPZ（筛选后的）
            {
                Console.WriteLine("MPZ初始化失败！");
                return;//异常结束
            }
            int i = 0;
            while (UpdateMatrix(matrix, dicMpz, dicSta, m_dmaxstop) != 0)
            {
                i++;//迭代次数
            }
            int nm = 0;
            double avev = 0;
            double avea = 0;
            double avec = 0;
            double navev = 0;
            double navea = 0;
            double navec = 0;
            measure(dicMpz, dicSta, ref avev, ref avea, ref avec, ref navev, ref navea, ref navec, ref nm);
            int rep = ClearRepZone(dicMpz, dicSta);//去除重复地域
            Console.WriteLine("最小移动数目：{0},地域相邻阈值：{1},停止迭代阈值：{2},迭代次数：{3},删除地域个数：{4}", m_iMinMoveVal, m_dzonemax, m_dmaxstop, i, rep);
            //filew1 << z;
            Console.WriteLine("新MPZ：{0},平均v：{1},平均a：{2},平均c：{3},新平v：{4},	新平a：{5},	新平c：{6}",  nm, avev, avea, avec, navev, navea, navec);
            // filew2 << m;
        }
        /// <summary>
        /// 去除重复地域
        /// </summary>
        /// <param name="dicMpz"></param>
        /// <param name="dicSta"></param>
        /// <returns></returns>
        private static int ClearRepZone(Dictionary<int, MPZ> dicMpz, Dictionary<int, Zone> dicSta)
        {
            List<int> Sta = new List<int>();
            int mn = dicMpz.Count;
            for (int ii = 0; ii < mn; ii++)
            {
                int i = dicMpz.ElementAt(ii).Key;
                Sta.Add(dicMpz[i].SId);
                Sta = Sta.Distinct().ToList();//去重
                Sta.Add(dicMpz[i].EId);
                Sta.Distinct().ToList();//去重
            }
            int count = 0;
            int zn = dicSta.Count;
            for (int i = 101; i <= zn; i++)
            {
                if (!Sta.Contains(dicMpz[i].PatternId))//不包含移除？
                {
                    dicMpz.Remove(i);
                    count++;//?
                    zn--;//?
                    i--;//?
                }

            }
            return count;
        }

        /// <summary>
        /// MPZ评估 
        /// </summary>
        /// <param name="dicMpz"></param>
        /// <param name="dicSta"></param>
        /// <param name="avev"></param>
        /// <param name="avea"></param>
        /// <param name="avec"></param>
        /// <param name="navev"></param>
        /// <param name="navea"></param>
        /// <param name="navec"></param>
        /// <param name="nm"></param>
        public static void measure(Dictionary<int, MPZ> dicMpz, Dictionary<int, Zone> dicSta, ref double avev, ref double avea, ref double avec, ref double navev, ref double navea, ref double navec, ref int nm)
        {
            int ch, ru;
            for (int i = 0; i < 100; i++)
            {
                ch = 0; ru = 0;
                for (int j = 0; j < 100; j++)
                {
                    ch += Data.moveamount[i][j];//第i+1个站点到每个站点的轨迹数目
                    ru += Data.moveamount[j][i];//每个站点到第i个站点的轨迹数目，站点间的轨迹数为有向图
                }
                dicSta[i+1].OutD = ch;//出度
                dicSta[i+1].InD = ru;//入度
            }
            int n = dicMpz.Count;
            nm = 0;
            double sumv = 0;
            double suma = 0;
            double sumc = 0;
            double nsumv = 0;
            double nsuma = 0;
            double nsumc = 0;
            for (int ii =0; ii <n; ii++)
            {
                int i = dicMpz.ElementAt(ii).Key;//返回序列中指定索引处的元素
                ch = dicSta[dicMpz[i].SId].OutD;
                ru = dicSta[dicMpz[i].EId].InD;
                dicMpz[i].V = (double)dicMpz[i].MoveNum / MoveRcd.movenum;//覆盖度
                dicMpz[i].A = (double)(dicMpz[i].MoveNum * MoveRcd.movenum) / (ch * ru);//精准度
                dicMpz[i].C = (double)Math.Sqrt(dicMpz[i].V * dicMpz[i].A);//权衡值
                sumv += dicMpz[i].V;
                suma += dicMpz[i].A;
                sumc += dicMpz[i].C;
                if (dicMpz[i].PatternId >= 9000)
                {
                    nsumv += dicMpz[i].V;
                    nsuma += dicMpz[i].A;
                    nsumc += dicMpz[i].C;
                    nm++;
                }
            }
            avev = sumv / n;
            avea = suma / n;
            avec = sumc / n;
            navev = nsumv / nm;
            navea = nsuma / nm;
            navec = nsumc / nm;
        }


        /// <summary>
        /// 更新矩阵
        /// </summary>
        /// <returns></returns>
        public static int UpdateMatrix(double[,] matrix, Dictionary<int, MPZ> dicMpz, Dictionary<int, Zone> dicSta, double epsi)
        {
            stop1 = dicMpz.Count;
            if (matrix == null)
            {
                matrix = BuildMatrix(dicMpz, dicSta);
            }
            int mi = 0, mj = 0;
            double max = getMax(dicMpz,matrix, ref mi, ref mj);
            if (max < epsi || stop1 == stop2)
            {
                return 0;//连接值小于阈值或者不能进行合并，停止迭代
            }
            int n1 = 1, n2 = 1;
            int szonid1 = dicMpz[mi].SId;//mi的上客站点标识码,返回序列中指定索引
            int ezonid1 = dicMpz[mi].EId;
            int szonid2 = dicMpz[mj].SId;
            int ezonid2 = dicMpz[mj].EId;
            int startz = 0, endz = 0;
            //新地域的站点:两个起始站点合并
            if (szonid1 != szonid2)
            {
                startz = zone_merge(dicSta, szonid1, szonid2, ref n1);
            }
            else
                startz = szonid1;
            //新地域的站点:两个终点站点合并
            if (ezonid1 != ezonid2)
            {
                /*if(szonid1==ezonid1&&szonid2==ezonid2)
                    endz = startz;*/
                endz = zone_merge(dicSta, ezonid1, ezonid2, ref n2);
            }
            else
                endz = ezonid1;
            //更新移动表
            //把第mi个删除，并取出值
            int tracnum = 0;
            MPZ mpzMove = dicMpz[mi];//把第mi个值取出
            dicMpz.Remove(mi);//把第mi个删除
            tracnum = mpzMove.MoveNum;
            mi = mpzMove.PatternId;
            Data.move_num[szonid1 - 1][ezonid1 - 1] = 0;
            //把第mj个删除，并取出值
            mpzMove = dicMpz[mj];//把第mj个值取出
            dicMpz.Remove(mj);//把第mj个删除
            tracnum += mpzMove.MoveNum;
            mj = mpzMove.PatternId;
            Data.move_num[szonid2 - 1][ezonid2 - 1] = 0;

            MPZ nmpz = new MPZ();
            nmpz.PatternId = moveid++;
            nmpz.SId = startz;//合并后的起点ID
            nmpz.EId = endz;//合并后的终点ID
            nmpz.MoveNum = tracnum;//移动模式包含移动记录的数量
            nmpz.MoveNos.Add(mi);//导入两条搭乘记录所属的移动号
            nmpz.MoveNos.Add(mj);

            //出入度
            dicSta[startz].OutD += tracnum;//起点出度
            dicSta[startz].InD += tracnum;//终点入度
            tracnum = 0;

            //找子集
            //子集
            MPZ mpzSubMove = new MPZ();//mpzSubMove为子集
            for (int i = 0; i < n1; i++)//n1是m1起点的个数
                for (int j = 0; j < n2; j++)//n2是m1终点的个数
                {
                    mpzSubMove.SId = dicSta[startz].StaNos[i];
                    mpzSubMove.EId = dicSta[endz].StaNos[j];
                    if ((tracnum = Data.move_num[mpzSubMove.SId - 1][mpzSubMove.EId - 1]) != 0)//将子集的移动数目赋值给tracnum且不为零
                    {
                        nmpz.MoveNum += tracnum;//合并子集移动数目
                        dicMpz.Remove(mpzSubMove.PatternId);//移除子集
                        Data.move_num[mpzSubMove.SId - 1][mpzSubMove.EId - 1] = 0;
                        dicSta[startz].OutD += tracnum;
                        dicSta[endz].InD += tracnum;
                    }

                }
            dicMpz.Add(dicMpz.Max(x=>x.Key)+1, nmpz);//将nmpz添加到dicMpz.Count的位置
            BuildMatrix(dicMpz, dicSta);
            stop2 = stop1;
            return 2;
        }
        /// <summary>
        /// 合并地域
        /// </summary>
        /// <param name="dicSta"></param>
        /// <param name="zid1"></param>
        /// <param name="zid2"></param>
        /// <param name="n"></param>
        /// <returns></returns>
        public static int zone_merge(Dictionary<int, Zone> dicSta, int zid1, int zid2, ref int n)
        {
            int n1 = dicSta[zid1].StaNos.Count();//区域站点标识码的个数
            int n2 = dicSta[zid2].StaNos.Count();
            int k = 0, temp = 0;
            List<int> Sta = new List<int>();
            for (int i = 0; i < n1; i++)
            {
                Sta.Add(dicSta[zid1].StaNos[i]);//将站点标识码存进去
            }
            Sta.Distinct().ToList();//去重
            for (int i = 0; i < n2; i++)
            {
                Sta.Add(dicSta[zid2].StaNos[i]);//将站点标识码存进去
            }
            Sta = Sta.Distinct().ToList();//去重
            Zone zone = new Zone();
            zone.ZoneId = dicSta.Count + 1;//初始化合并区域的ID
            //zone.StaNum = Sta.Count();//只读，自动赋值
            n = zone.StaNum;
            //检查是否已经产生这个区，从102开始和新产生的比较
            for (int j = 101; j < zone.ZoneId; j++)
            {
                if (zone.StaNum != dicSta[j].StaNum) continue;//组成该地域的站点的数目不同，产生新区
                for (k = 1; k <= dicSta[j].StaNum; k++)
                {
                    if (!Sta.Contains(dicSta[j].StaNos[k]))
                    {
                        break;//组成该地域的站点的数目相同且找不到站点标识码则break，此时k<=Sta_Num
                    }
                }
                if (k > dicSta[j].StaNum)//如果找得到则删除该站点，并返回和他一样区域的ID
                {
                    return dicSta[j].ZoneId;
                }
            }
            //将新产生的地域中包含的站点添加进来
            for (int p = 1; p <= zone.StaNum; p++)
            {
                temp = Sta[p];
                zone.StaNos.Add(temp);
            }
            dicSta.Add(dicSta.Max(x => x.Key) + 1, zone);//将新地域加到最后
            return zone.ZoneId;//返回新地域ID
        }
      /// <summary>
        /// 求连接矩阵中的最大值
      /// </summary>
      /// <param name="dicMpz"></param>
      /// <param name="matrix"></param>
      /// <param name="mi"></param>
      /// <param name="mj"></param>
      /// <returns></returns>

        public static double getMax( Dictionary<int, MPZ> dicMpz,double[,] matrix, ref int mi, ref int mj)
        {
            double max = 0;
            for (int i = 0; i < matrix.GetLength(0); i++)
            {
                for (int j = 0; j < matrix.GetLength(1); j++)
                {
                    if (matrix[i, j] > max)
                    {
                        max = matrix[i, j];
                        mi = dicMpz.ElementAt(i).Key;//将索引为i的PatternId赋给mi
                        mj = dicMpz.ElementAt(j).Key;
                    }
                }
            }
            return max;
        }
        /// <summary>
        /// 建立矩阵
        /// </summary>
        /// <param name="dicMpz"></param>
        /// <param name="dicSta"></param>
        /// <returns></returns>
        public static double[,] BuildMatrix(Dictionary<int, MPZ> dicMpz, Dictionary<int, Zone> dicSta)
        {
            //string str2 = "../../../move_rcd2.txt";
            //List<MoveRcd> dicMov = MoveRcd.move_Ini(str2, m_iMinMoveVal);
            int n = dicMpz.Count;//336
            //string[,] arr = new string[12, 31] 
            double[,] matrix = new double[n, n];//初始化
            for (int i = 0; i <n; i++)
            {
                for (int j = i; j < n; j++)
                {
                    matrix[i,j] = CountEle(dicMpz.ElementAt(i).Value, dicMpz.ElementAt(j).Value, dicSta); //设置第i行,第j列的值等于CountEle(*m1,*m2,z)
                }
            }
            return matrix;
        }
        //////////////////////矩阵计算//////////////////////
        //计算矩阵元素值（计算任意两个MPZ间连接值的连接矩阵），m1和m2就是两条MPZ
        public static double CountEle(MPZ m1, MPZ m2, Dictionary<int, Zone> zlist)
        {
            if (m1.PatternId == m2.PatternId) return 0;//同一条轨迹取0
            if (!Zone.zone_dis(zlist[m1.SId], zlist[m2.SId], m_dzonemax)) return 0;//起点不相邻取0
            if (!Zone.zone_dis(zlist[m1.EId], zlist[m2.EId], m_dzonemax)) return 0;//终点不相邻取0
            long n1 = zlist[m1.SId].StaNos.Count();//m1里面起点包含站点的个数
            long n2 = zlist[m1.EId].StaNos.Count();//m1里面终点包含站点的个数
            long n3 = zlist[m2.SId].StaNos.Count();
            long n4 = zlist[m2.EId].StaNos.Count();
            long cnt = 0, n5 = 0, n6 = 0;
            List<int> S_Sta = new List<int>();//m1和m2中的起点的站点标识码
            List<int> E_Sta = new List<int>();//m1和m2中的终点的站点标识码

            //起点站点去重s
            for (int i = 0; i < n1; i++)
                S_Sta.Add(zlist[m1.SId].StaNos[i]);//add可能会重复添加
            for (int i = 0; i < n3; i++)
                S_Sta.Add(zlist[m2.SId].StaNos[i]);
            S_Sta = S_Sta.Distinct().ToList();//去重

            //终点站点去重
            for (int i = 0; i < n2; i++)
                E_Sta.Add(zlist[m1.EId].StaNos[i]);
            for (int i = 0; i < n4; i++)
                E_Sta.Add(zlist[m2.EId].StaNos[i]);
            E_Sta = E_Sta.Distinct().ToList();
            n5 = S_Sta.Count();//起点个数
            n6 = E_Sta.Count();//终点个数
            for (int i = 0; i < n5; i++)
                for (int j = 0; j < n6; j++)
                    cnt += Data.moveamount[S_Sta [i] - 1][E_Sta[j] - 1];
            return ((double)cnt) / (n5 * n6);//两个MPZ间的连接值，n5*n6表示起点到终点的总移动数目
        }
    }
}
