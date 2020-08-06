using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
namespace Tools
{
    public class WinForm
    {

        /// <summary>
        /// 刷新一个盒子，并且放入新内容
        /// </summary>
        /// <param name="list">盒子实体</param>
        /// <param name="box">新内容</param>
        public static void MakeBox(List<String> list, ComboBox box)
        {
            //清空盒子
            box.Items.Clear();
            //向盒子加入元素
            for (int i = 0; i < list.Count; i++)
            {
                box.Items.Add(list[i]);
            }
        }

        /// <summary>
        /// 刷新一个列表，并且放入新内容
        /// </summary>
        /// <param name="list">列表实体</param>
        /// <param name="box">新内容</param>
        public static void MakeList(List<String> list, ListBox box)
        {
            if (list == null || box == null)
                return;

            //清空列表
            box.Items.Clear();
            //向列表加入元素
            for (int i = 0; i < list.Count; i++)
            {
                box.Items.Add(list[i]);
            }
        }
    }
}
