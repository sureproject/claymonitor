using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

using System.Net;
using System.Text.RegularExpressions;
using Microsoft.Win32;
using System.IO;
using System.Management;
using OpenHardwareMonitor.Hardware;
using OpenHardwareMonitor.Collections;
using System.Web.Script.Serialization;

namespace Claymonitor
{
    public partial class frmMain : Form
    {
        int second_refresh = 0;
        int second_post = 0;
        int counter = 0;
        int counter_line = 0;
        string status_bar = "";
        int rig_id = 0;
        public static string gpu_name;

        string pool = "";
        string coin = "";
        int num_gpu = 0;
        string total_speed = "";
        string total_share = "";
        string total_reject = "";
        int total_time_m = 0;
        string hashrate_unit = "";
        string total_time = "";

        int interval_refresh = 10; // second
        int interval_post = 60*60; // second * min for post to sure.in.th/miner
        int interval_line_post = 60;
        string gpus_line = "";
        string rig_profit = "";

        string get_time = "";
        string post_time = "";
        int line_notify = 0;
        int low_hashrate = 50;
        int low_hashrate_delay = 0;

        string gpus_data = "";

        RegistryKey reg = Registry.CurrentUser.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run",true);

        // time 2
        int interval_get_coin_data = 360;
        //int interval_get_coin_rank = 360;
        int second_get = 0;

        public frmMain()
        {
            InitializeComponent();
        }

        private static string GetComponent(string hwClass, string syntax)
        {
            int i = 0;
            string info = "";
            
            ManagementObjectSearcher mos = new ManagementObjectSearcher("root\\CIMV2","SELECT * FROM "+hwClass);
            foreach(ManagementObject mj in mos.Get())
            {
                //MessageBox.Show(i+". "+Convert.ToString(mj[syntax]));
                if(i==0) info = Convert.ToString(mj[syntax].ToString());
                else info =  info + "," + Convert.ToString(mj[syntax].ToString());
                i++;
            }

            return info.ToString();
        }

        private void frmMain_Load(object sender, EventArgs e)
        {
            timer1.Interval = 1000;
            timer1.Start();
            timer2.Interval = 1000;
            timer2.Start();

            int item = 12;


            textBox1.Text = Properties.Settings.Default.rig_id.ToString();
            textBox2.Text = Properties.Settings.Default.server.ToString();
            textBox3.Text = Properties.Settings.Default.port.ToString();
            textBox4.Text = Properties.Settings.Default.line_token.ToString();
            textBox5.Text = Properties.Settings.Default.rig_power.ToString();
            textBox6.Text = Properties.Settings.Default.rig_name.ToString();
            line_notify = Properties.Settings.Default.line_notify_use;

            upNum.Value = Properties.Settings.Default.interval_refresh;
            interval_refresh = Int32.Parse(upNum.Value.ToString());

            upLowHashrate.Value = Properties.Settings.Default.low_hashrate;
            low_hashrate = Properties.Settings.Default.low_hashrate;

            // line notify
            upLine.Value = Properties.Settings.Default.line_notify_interval;
            interval_line_post = Int32.Parse(upLine.Value.ToString())*60;


            if (Properties.Settings.Default.line_notify_use == 1)
            {
                cbLine.Checked = true;
            }
            else
            {
                cbLine.Checked = false;
            }


            if (Properties.Settings.Default.autostart == 1)
            {
                cbAutostart.Checked = true;
            }
            else
            {
                cbAutostart.Checked = false;
            }


            // get rig id by rig code
            string cURL = "https://sure.in.th/miner/code2id.php?code=" + textBox1.Text.ToString().Trim();
            using (WebClient client = new WebClient())
            {
                try
                {
                    string web_data = client.DownloadString(cURL);
                    web_data = web_data.ToString().Trim();
                    if (web_data.Equals("")) web_data = "0";
                    rig_id = Int32.Parse(web_data);
                }
                catch (Exception ex)
                {
                    ex.ToString();
                }
            }

            // get claymore api data
            string sURL = "http://" + textBox2.Text.Trim() + ":" + textBox3.Text.Trim();

            using (WebClient client = new WebClient())
            {
                try
                {
                    string web_data = client.DownloadString(sURL);
                    string pool = get_string_between(web_data, "New job from", "</font>");
                    string[] arr = Regex.Split(web_data, "</font><br><font color=\"#00ff00\">GPU #0");
                    string data = "GPU #0" + arr[2];
                    string coin = get_string_between(data, "Current ", "share target");
                    string tmp_gpu = get_string_between(data, "color=\"#ff00ff\">", "</font>");
                    string[] arr_gpu = Regex.Split(tmp_gpu, "%");
                    Array.Resize(ref arr_gpu, arr_gpu.Length - 1);

                    item = arr_gpu.Count();

                }
                catch
                {
                    item = 12;
                }
            }


            for (int i = 0; i < item; i++)
            {
                dataGridView1.Rows.Add();
            }

            for (int i = 0; i < 3; i++)
            {
                dataGridView2.Rows.Add();
            }

            for (int i = 0; i < 9; i++)
            {
                dataGridView3.Rows.Add();
            }

            //label8.Text = GetComponent("Win32_VideoController", "Name");

            /*Computer c = new Computer(){GPUEnabled = true};
            c.Open();
            foreach(var hardware in c.Hardware)
            {
                //if(hardware.HardwareType == HardwareType.GpuNvidia)
                //{
                    hardware.Update();
                    foreach(var sensor in hardware.Sensors)
                    {
                        if(sensor.SensorType == SensorType.Data)
                        {
                            //MessageBox.Show(sensor.Name.ToString() + " : " + sensor.Hardware.ToString() + " : " + sensor.SensorType.ToString() + " = "  + sensor.Value.GetValueOrDefault());
                        }

                    }
                    
                //}
                
            }*/
            

        }

        private static string get_string_between(string str, string start_txt,string end_txt)
        {
	        string[] arr = Regex.Split(str, start_txt);
            string[] arr2 = Regex.Split(arr[1], end_txt);
            return arr2[0];
        }

        public static string get_between(string strSource, string strStart, string strEnd)
        {
            int Start, End;
            if (strSource.Contains(strStart) && strSource.Contains(strEnd))
            {
                Start = strSource.IndexOf(strStart, 0) + strStart.Length;
                End = strSource.IndexOf(strEnd, Start);
                return strSource.Substring(Start, End - Start);
            }
            else
            {
                return "";
            }
        }


        private void lineNotify(string msg ,string token = "")
        {
            try
            {
                var request = (HttpWebRequest)WebRequest.Create("https://notify-api.line.me/api/notify");
                var postData = string.Format("message={0}", msg);
                var data = Encoding.UTF8.GetBytes(postData);

                request.Method = "POST";
                request.ContentType = "application/x-www-form-urlencoded";
                request.ContentLength = data.Length;
                request.Headers.Add("Authorization", "Bearer " + token);

                using (var stream = request.GetRequestStream()) stream.Write(data, 0, data.Length);
                var response = (HttpWebResponse)request.GetResponse();
                var responseString = new StreamReader(response.GetResponseStream()).ReadToEnd();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }

        class CoinData
        {
            public string data_time { get; set; }
            public string rev_profit { get; set; }
            public string power_cost { get; set; }
            public string profit { get; set; }
        }
        class Coin
        {
            public string coin { get; set; }
            public string hashrate { get; set; }
            public string power { get; set; }
            public string elect_cost { get; set; }
            public string coin_price { get; set; }
            public List<CoinData> items { get; set; }
        }

        class CoinDataRank
        {
            public string rank { get; set; }
            public string coin { get; set; }
            public string rev_profit { get; set; }
            public string power_cost { get; set; }
            public string profit { get; set; }
        }
        class CoinRank
        {
            public string rig_id { get; set; }
            public List<CoinDataRank> items { get; set; }
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
           
            
            
            


            label1.Text = DateTime.Now.ToString();
            statusBar1.Text = "";

            second_refresh = second_refresh + 1;
            label4.Text = ((interval_refresh - second_refresh + 1)+1).ToString();

            

            TimeSpan t = TimeSpan.FromSeconds(counter);
            string time_txt = string.Format("{0:D2}:{1:D2}:{2:D2}",
                                            t.Hours,
                                            t.Minutes,
                                            t.Seconds);

            string[] tmp_time = Regex.Split(time_txt.ToString(), ":");
            time_txt = tmp_time[0] + " ชั่วโมง " + tmp_time[1] + " นาที " + tmp_time[2] + " วินาที";

            

            if (second_refresh % interval_refresh == 1 )
            {
                get_time = DateTime.Now.ToString();
                second_refresh = 1;

                

                // read claymore data
                string sURL = "http://" + textBox2.Text.Trim() + ":" + textBox3.Text.Trim();

                using (WebClient client = new WebClient())
                {
                    try
                    {

                        string web_data = client.DownloadString(sURL);

                        pool = get_string_between(web_data, "New job from", "</font>");
                        string[] arr = Regex.Split(web_data, "</font><br><font color=\"#00ff00\">GPU #0");
                        string data = "GPU #0"+arr[2];
                        coin = get_string_between(data, "Current ", "share target");
                        string tmp_gpu = get_string_between(data, "color=\"#ff00ff\">", "</font>");
                        string[] arr_gpu = Regex.Split(tmp_gpu, "%");
                        Array.Resize(ref arr_gpu, arr_gpu.Length - 1);

                        num_gpu = arr_gpu.Count();
                        total_speed = get_string_between(data, "Total Speed: ", " ");
                        string speed_unit = get_string_between(data, "Total Speed: "+total_speed, ",");
                        string tmp_share = get_string_between(data, "Total Speed: ", ", Rejected");
                        tmp_share = tmp_share.Replace(total_speed.ToString()+speed_unit.ToString()+", Total", "");
                        tmp_share = tmp_share.Replace("(", "xxx");
                        total_share = get_string_between(tmp_share, "Shares: ", "xxx");
                        total_reject = get_string_between(data, "Rejected: ", ",");
                        total_time = get_string_between(data, "Time:", "\n</font>");
                        string[] arr_time = Regex.Split(total_time, ":");
                        total_time_m = Int32.Parse(arr_time[0]) * 60 + Int32.Parse(arr_time[1]);
                        total_time = total_time.Replace(":", " ชั่วโมง ")+" นาที";

                        string each_share = get_string_between(data, "Total Shares: ", ", Rejected:");
                        each_share = each_share.Replace(total_share.ToString()+"(", "").Replace(")","").Replace("+",",");
                        string[] arr_share = Regex.Split(each_share, ",");

                        string tmp_hr =get_string_between(data, "Time:", "</font><br><font color=\"#00ffff\">Incorrect");
                        tmp_hr = get_string_between(tmp_hr, "00ffff\">", "</font>");

         
                        lbPool.Text = "POOL : " + pool.ToString();
                        lbCoin.Text = "COIN : " + coin.ToString();
                        lbGPU.Text = "GPU : " + num_gpu.ToString();
                        lbSpeed.Text = "SPEED : " + total_speed.ToString()+" "+speed_unit;
                        lbShare.Text = "SHARE : " + total_share.ToString();
                        lbReject.Text = "REJECT : " + total_reject.ToString();
                        lbOnline.Text = "ONLINE : " + total_time.ToString();

                        gpus_line = "--------------------------------------------\n";
                        gpus_line = gpus_line + "GPU#|Hash|Share|Sh%25|Temp|Fan\n";
                        gpus_line = gpus_line + "--------------------------------------------\n";

                        gpus_data = "{\"gpus\":{";

                        for (int i = 0; i < num_gpu; i++)
                        {
                            arr_gpu[i] = arr_gpu[i]+"%";
                            string gpu_temp = get_string_between(arr_gpu[i], "t=", "C");
                            string gpu_fan = get_string_between(arr_gpu[i], "fan=", "%");
                             
                            string g = "GPU #"+i+"";
                            string gg = get_string_between(data,g, "units");
                            string gpu_series =  get_string_between(gg, ":", ",");

                            string[] gm = Regex.Split(get_string_between(gg, ":", "MB"), ",");
                            string gpu_memory = gm[1];

                            string[] gc = Regex.Split(get_string_between(gg, ":", "compute"), ",");
                            string gpu_compute_unit = gc[2];

                            int m = Int32.Parse(gpu_memory) / 1024;
                            string mem = "";
                            if (m > 0) mem = " "+m+"GB";

                            string gpu_model = "";

                            if (gpu_series.Trim().Equals("Ellesmere") && gpu_compute_unit.Trim().Equals("32")) gpu_model = "AMD RX 570"+ mem;
			                else if (gpu_series.Trim().Equals("Ellesmere") && gpu_compute_unit.Trim().Equals("36")) gpu_model = "AMD RX 580"+mem;
			                else if (gpu_series.Trim().Equals("Ellesmere Pro")) gpu_series = "AMD RX 470" + mem;
			                else if (gpu_series.Trim().Equals("Ellesmere XT")) gpu_series = "AMD RX 480" + mem;
			                else if (gpu_series.Trim().Equals("Ellesmere")) gpu_series = "AMD" + mem;
			                else gpu_model = gpu_series;

                            string[] arr_hr = Regex.Split(get_string_between(tmp_hr, "GPU"+i, ",").Trim()," ");
                            string gpu_hashrate = arr_hr[0];
                            hashrate_unit = arr_hr[1];
                            double share_percent = double.Parse(arr_share[i]) / double.Parse(total_share) * 100.00;

                            dataGridView1.Rows[i].Cells[0].Value = (i+1).ToString();
                            dataGridView1.Rows[i].Cells[1].Value = "GPU"+(i).ToString();
                            dataGridView1.Rows[i].Cells[2].Value = gpu_model.ToString();
                            dataGridView1.Rows[i].Cells[3].Value = gpu_hashrate.ToString() + " " + hashrate_unit.ToString();
                            dataGridView1.Rows[i].Cells[4].Value = arr_share[i].ToString();
                            dataGridView1.Rows[i].Cells[5].Value = share_percent.ToString("F")+"%";
                            dataGridView1.Rows[i].Cells[6].Value = gpu_temp.ToString() + " °C";
                            dataGridView1.Rows[i].Cells[7].Value = gpu_fan.ToString() + "%";

                            gpus_data = gpus_data + "\"gpu"+(i)+"\":{";

                            gpus_data = gpus_data + "\"gpu_code\":\""+ "GPU" + (i).ToString() + "\",";
                            gpus_data = gpus_data += "\"gpu_name\":\""+ gpu_model.ToString() + "\",";
                            gpus_data = gpus_data + "\"hashrate\":"+ gpu_hashrate.ToString()+",";
                            gpus_data = gpus_data + "\"hashrate_unit\":\""+ hashrate_unit.ToString()+"\",";
                            gpus_data = gpus_data + "\"share\":"+ arr_share[i].ToString()+",";
                            gpus_data = gpus_data + "\"share_percent\":\""+ share_percent.ToString("F") +"\",";
                            gpus_data = gpus_data + "\"temperature\":"+ gpu_temp.ToString()+",";
                            gpus_data = gpus_data + "\"fan_speed\":"+ gpu_fan.ToString()+"";

                            gpus_data = gpus_data + "},";

                            
                            gpus_line = gpus_line + "GPU"+i+ "|"+ gpu_hashrate.ToString()+"|" + arr_share[i].ToString()+"|"+ share_percent.ToString("F")+ "|" + gpu_temp.ToString()+ "c|" + gpu_fan.ToString()+ "\n";

                        }
                        gpus_data = gpus_data.Remove(gpus_data.Trim().Length - 1);
                        gpus_data = gpus_data + "}}"; // gpus


                        

                        // send line notify if hashrate low 
                        if( Double.Parse( total_speed.ToString()) < Double.Parse(low_hashrate.ToString()) && line_notify == 1 && textBox4.Text.Trim().ToString().Length > 0 && low_hashrate_delay == 0) {
                            //MessageBox.Show(gpus_line);
                            string msg = "Claymonitor Report\n";
                            msg = msg + textBox6.Text.ToString() + " : ทำงาน " + total_time.ToString() + "\n";
                            msg = msg + "Shares : " + total_share.ToString() + " Rejected : " + total_reject.ToString() + "\n";
                            msg = msg + "รวมแรงทั้งหมด " + total_speed.ToString() + " " + hashrate_unit.ToString() + "\n";
                            msg = msg + "-- แรงขุดน้อยกว่า "+ low_hashrate+ " "+ hashrate_unit.ToString() + "\n";
                            msg = msg + gpus_line;
                            msg = msg + "--------------------------------------------\n";
                            if (rig_id > 0) msg = msg + "ดูเพิ่ม https://sure.in.th/miner/rig-detail.php?id=" + rig_id;

                            //MessageBox.Show(msg);
                            lineNotify(msg, textBox4.Text.Trim().ToString());
                            low_hashrate_delay = 1;
                        }

                        if(Double.Parse(total_speed.ToString()) >= Double.Parse(low_hashrate.ToString()) && low_hashrate_delay == 1)
                        {
                            //MessageBox.Show(gpus_line);
                            string msg = "Claymonitor Report\n";
                            msg = msg + textBox6.Text.ToString() + " : ทำงาน " + total_time.ToString() + "\n";
                            msg = msg + "Shares : " + total_share.ToString() + " Rejected : " + total_reject.ToString() + "\n";
                            msg = msg + "รวมแรงทั้งหมด " + total_speed.ToString() + " " + hashrate_unit.ToString() + "\n";
                            msg = msg + "** แรงขุดกลับมามากกว่า " + low_hashrate + " " + hashrate_unit.ToString() + "\n";
                            msg = msg + gpus_line;
                            msg = msg + "--------------------------------------------\n";
                            if (rig_id > 0) msg = msg + "ดูเพิ่ม https://sure.in.th/miner/rig-detail.php?id=" + rig_id;

                            //MessageBox.Show(msg);
                            lineNotify(msg, textBox4.Text.Trim().ToString());
                            low_hashrate_delay = 0;
                        }

                        status_bar = "เชื่อมต่อ server สำเร็จ " + get_time.ToString();

                    }
                    catch 
                    {
                        status_bar = "ไม่สามารถเชื่อมต่อกับ server ได้ " + get_time.ToString();
                    }
                }
                
            }

            

          
            second_get++;
            if (second_get % interval_get_coin_data == 1)
            {
                //MessageBox.Show("Get data");
                // get rig id by rig code
                string mURL = "https://sure.in.th/miner/coin-json.php?id="+coin.Trim().ToString().ToLower()+"&hashrate="+total_speed.Trim().ToString()+"&power="+textBox5.Text.Trim().ToString();
                using (WebClient client = new WebClient())
                {
                    try
                    {
                        string web_data = client.DownloadString(mURL);
                        web_data = web_data.ToString().Trim();
                        if (web_data.Length > 50)
                        {   
                            var coin_data = new JavaScriptSerializer().Deserialize<Coin>(web_data);
                            //MessageBox.Show(web_data);

                            //byte[] bytes = Encoding.Default.GetBytes(coin_data.items[0].data_time.ToString());
                            //string utf8_String = Encoding.UTF8.GetString(bytes);

                            //MessageBox.Show(coin_data.items[0].revence.ToString());

                            for(int i = 0;i < 4; i++)
                            {
                                dataGridView2.Rows[i].Cells[0].Value = coin_data.items[i].data_time.ToString();
                                dataGridView2.Rows[i].Cells[1].Value = coin_data.items[i].rev_profit.ToString();
                                dataGridView2.Rows[i].Cells[2].Value = coin_data.items[i].power_cost.ToString();
                                dataGridView2.Rows[i].Cells[3].Value = coin_data.items[i].profit.ToString();
                            }

                            rig_profit = "ขุดเหรียญ : "+coin_data.coin.ToUpper()+" ("+coin_data.coin_price.ToString()+" บาท)\n";
                            rig_profit = rig_profit + "รายได้ต่อวัน : " + coin_data.items[0].rev_profit.ToString()+" บาท\n";
                            rig_profit = rig_profit+"ค่าไฟต่อวัน : " + coin_data.items[0].power_cost.ToString() + " บาท\n";
                            rig_profit = rig_profit + "กำไรต่อวัน : " + coin_data.items[0].profit.ToString() + " บาท\n";

                        } 
                        
                    }
                    catch (Exception ex)
                    {
                        ex.ToString();
                    }
                }

                // get coin rank to mine
                if (textBox1.Text.Trim().ToString().Length > 10)
                {
                    string rURL = "https://sure.in.th/miner/rig-json.php?id=" + rig_id;
                    using (WebClient client = new WebClient())
                    {
                        try
                        {
                            string web_data = client.DownloadString(rURL);
                            web_data = web_data.ToString().Trim();
                            if (web_data.Length > 50)
                            {
                                var coin_data = new JavaScriptSerializer().Deserialize<CoinRank>(web_data);
                                //MessageBox.Show(web_data);

                                //byte[] bytes = Encoding.Default.GetBytes(coin_data.items[0].data_time.ToString());
                                //string utf8_String = Encoding.UTF8.GetString(bytes);

                                //MessageBox.Show(coin_data.items[0].revence.ToString());

                                for (int i = 0; i < 10; i++)
                                {
                                    dataGridView3.Rows[i].Cells[0].Value = coin_data.items[i].rank.ToString();
                                    dataGridView3.Rows[i].Cells[1].Value = coin_data.items[i].coin.ToString();
                                    dataGridView3.Rows[i].Cells[2].Value = coin_data.items[i].rev_profit.ToString();
                                    dataGridView3.Rows[i].Cells[3].Value = coin_data.items[i].power_cost.ToString();
                                    dataGridView3.Rows[i].Cells[4].Value = coin_data.items[i].profit.ToString();
                                }

                            }

                        }
                        catch (Exception ex)
                        {
                            ex.ToString();
                        }
                    }
                } // end check have rig code

            }



            

            
            counter += 1;
            statusBar1.Text = status_bar.ToString();

            //lbDebug.Text = "["+ time_txt.ToString()+"] "+ tmp_hr.ToString();
            
            lbDebug.Text = "อัพเดทข้อมูลล่าสุด  " + get_time.ToString() + "\n";
            lbDebug.Text = lbDebug.Text + "โพสข้อมูลล่าสุด       " + post_time.ToString() + "\n";
            lbDebug.Text = lbDebug.Text + "เปิดใช้งานมาแล้ว   " + time_txt.ToString() + "\n";
            


        }

        private void timer2_Tick(object sender, EventArgs e)
        {
            //MessageBox.Show("Time2");
            // line notify on time
            counter_line++;
            second_post = second_post + 1;

            if (line_notify == 1 && counter_line % interval_line_post == 0 && textBox4.Text.ToString().Trim().Length > 0)
            {
                //MessageBox.Show("POST LINE");
                string msg = "Claymonitor Report\n";
                msg = msg + textBox6.Text.ToString() + " : ทำงาน " + total_time.ToString() + "\n";
                msg = msg + "Shares : " + total_share.ToString() + " Rejected : " + total_reject.ToString() + "\n";
                msg = msg + "รวมแรงทั้งหมด " + total_speed.ToString() + " " + hashrate_unit.ToString() + "\n";
                msg = msg + gpus_line;
                msg = msg + "--------------------------------------------\n" + rig_profit;
                msg = msg + "--------------------------------------------\n";

                if (rig_id > 0) msg = msg + "ดูเพิ่ม https://sure.in.th/miner/rig-detail.php?id=" + rig_id;

                //MessageBox.Show(msg);
                lineNotify(msg, textBox4.Text.Trim().ToString());
                //gpus_line = "";
                label16.Text = "ส่งข้อมูลทางไลน์     " + DateTime.Now.ToString();
            }

            // post data to server by interval
            if (second_post % interval_post == 1 && textBox1.Text.Trim().ToString().Length > 10) // run in x second (interval second)
            {
                //second_post = 0;
                post_time = DateTime.Now.ToString();

                // get rig id by rig code
                string pURL = "https://sure.in.th/miner/claymonitor-post.php?code=" + textBox1.Text.ToString().Trim() +
                    "&host=" + textBox2.Text.ToString().Trim() +
                    "&port=" + textBox3.Text.ToString().Trim() +
                    "&pool=" + pool.ToString().Trim() +
                    "&coin=" + coin.ToString() +
                    "&gpu=" + num_gpu.ToString() +
                    "&speed=" + total_speed.ToString() +
                    "&speed_unit=" + hashrate_unit.ToString() +
                    "&share=" + total_share.ToString() +
                    "&reject=" + total_reject.ToString() +
                    "&online=" + total_time_m.ToString() +
                    "&gpus=" + gpus_data.ToString();
                using (WebClient client = new WebClient())
                {
                    try
                    {
                        string web_data = client.DownloadString(pURL);
                        web_data = web_data.ToString().Trim();
                        //if (web_data.Equals("")) web_data = "0";
                        //MessageBox.Show(web_data);
                        //richTextBox1.Text = web_data;
                        status_bar = status_bar + " โพสข้อมูล " + get_time.ToString();
                    }
                    catch (Exception ex)
                    {
                        ex.ToString();
                    }
                }
                gpus_data = "";

            }

        }



        private void linkLabel1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            string sURL = "https://sure.in.th/miner/code2id.php?code="+ textBox1.Text;
            using (WebClient client = new WebClient())
            {
                try
                {
                    string web_data = client.DownloadString(sURL);
                    web_data = web_data.ToString().Trim();
                    if (web_data.Equals("")) web_data = "0";
                    VisitLink(web_data);
                }
                catch (Exception ex)
                {
                    ex.ToString();
                    MessageBox.Show("Unable to open link that was clicked.");
                }
            }
        }

        private void VisitLink(string id)
        {
            //Call the Process.Start method to open the default browser   
            //with a URL:  

            string sURL = "https://sure.in.th/miner/rig-detail.php?id="+id;
            System.Diagnostics.Process.Start(sURL);
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            Properties.Settings.Default.rig_id = textBox1.Text.Trim();
            Properties.Settings.Default.Save();
        }

        private void dataGridView1_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {

            
        }

        private void textBox2_TextChanged(object sender, EventArgs e)
        {
            Properties.Settings.Default.server = textBox2.Text.Trim();
            Properties.Settings.Default.Save();
        }

        private void textBox3_TextChanged(object sender, EventArgs e)
        {
            Properties.Settings.Default.port = textBox3.Text.Trim();
            Properties.Settings.Default.Save();
        }

        private void linkLabel2_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            string sURL = "https://notify-bot.line.me/my/";
            System.Diagnostics.Process.Start(sURL);
        }



        private void cbAutostart_CheckedChanged(object sender, EventArgs e)
        {
            if (cbAutostart.Checked)
            {
                reg.SetValue("claymonitor", Application.ExecutablePath.ToString());
                Properties.Settings.Default.autostart = 1;
                Properties.Settings.Default.Save();
            }
            else
            {
                reg.DeleteValue("claymonitor", false);
                Properties.Settings.Default.autostart = 0;
                Properties.Settings.Default.Save();
            }
        }

        private void upNum_ValueChanged(object sender, EventArgs e)
        {
            interval_refresh = Int32.Parse(upNum.Value.ToString());
            Properties.Settings.Default.interval_refresh = interval_refresh;
            Properties.Settings.Default.Save();
        }

        private void textBox4_TextChanged(object sender, EventArgs e)
        {
            // wCpyRPbtvML5g41K6d8EUUxpXI5Mnx1Fug2u1vYKR5f
            Properties.Settings.Default.line_token = textBox4.Text.ToString().Trim();
            Properties.Settings.Default.Save();
        }

        private void cbLine_CheckedChanged(object sender, EventArgs e)
        {
            if (cbLine.Checked)
            {
                line_notify = 1;
                Properties.Settings.Default.line_notify_use = 1;
                Properties.Settings.Default.Save();
            }
            else
            {
                line_notify = 0;
                Properties.Settings.Default.line_notify_use = 0;
                Properties.Settings.Default.Save();
            }
        }

        private void upLine_ValueChanged(object sender, EventArgs e)
        {
            interval_line_post = Int32.Parse(upLine.Value.ToString())*60;
            Properties.Settings.Default.line_notify_interval = Int32.Parse(upLine.Value.ToString());
            Properties.Settings.Default.Save();
        }

        private void textBox5_TextChanged(object sender, EventArgs e)
        {
            Properties.Settings.Default.rig_power = Int32.Parse(textBox5.Text);
            Properties.Settings.Default.Save();
        }

        private void textBox6_TextChanged(object sender, EventArgs e)
        {
            Properties.Settings.Default.rig_name = textBox6.Text.ToString();
            Properties.Settings.Default.Save();
        }

        private void upLowHashrate_ValueChanged(object sender, EventArgs e)
        {
            low_hashrate = Int32.Parse(upLowHashrate.Value.ToString());
            Properties.Settings.Default.low_hashrate = Int32.Parse(upLowHashrate.Value.ToString());
            Properties.Settings.Default.Save();
        }

        
    }
}
