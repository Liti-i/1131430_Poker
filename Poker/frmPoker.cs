using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Poker
{
    public partial class frmPoker : Form
    {
        #region 欄位
        /// <summary>
        /// 用來存放牌桌上五張牌的 PictureBox 陣列
        /// </summary>
        PictureBox[] pic = new PictureBox[5];

        /// <summary>
        /// 所有的牌的編號，從 0 到 51，對應到 52 張牌
        /// </summary>
        int[] allPoker = new int[52];

        /// <summary>
        /// 記錄玩家手牌的編號，從 0 到 51，對應到 52 張牌
        /// </summary>
        int[] playerPoker = new int[5];

        /// <summary>
        /// 玩家總資金
        /// </summary>
        private decimal totalMoney = 1000000;

        /// <summary>
        /// 玩家本局下注金額
        /// </summary>
        private decimal betAmount = 0;

        /// <summary>
        /// 判斷本局是否已下注
        /// </summary>
        private bool hasBetThisRound = false;

        /// <summary>
        /// 牌型賠率對照表
        /// </summary>
        private Dictionary<string, int> payoutOdds = new Dictionary<string, int>()
        {
            { "RoyalFlush", 250 },      // 皇家同花順
            { "StraightFlush", 50 },    // 同花順
            { "FourOfAKind", 25 },      // 四條
            { "FullHouse", 9 },         // 葫蘆
            { "Flush", 6 },             // 同花
            { "Straight", 4 },          // 順子
            { "ThreeOfAKind", 3 },      // 三條
            { "TwoPair", 2 },           // 兩對
            { "OnePair", 1 }            // 一對
        };

        #endregion

        public frmPoker()
        {

            InitializeComponent();
            InitializePoker();
            InitializeBetting();
        }


        #region 自定義方法
        private void InitializePoker()
        {
            for (int i = 0; i < pic.Length; i++)
            {
                pic[i] = new PictureBox();
                pic[i].Image = GetImage("back");
                pic[i].Name = "pic" + i;
                pic[i].SizeMode = PictureBoxSizeMode.AutoSize;
                pic[i].Top = 30;
                pic[i].Left = 10 + ((pic[i].Width + 10) * i);
                // 預設牌桌上的牌不可點擊
                pic[i].Enabled = false;
                // 預設牌桌上的牌的 Tag 為 "back"，表示牌面朝下
                pic[i].Tag = "back";
                pic[i].Visible = true;

                // 將 pic 丟至到 grpPorker 內
                this.grpPoker.Controls.Add(pic[i]);

                pic[i].Click += Pic_Click;
            }
        }

        /// <summary>
        /// 初始化下注系統
        /// </summary>
        private void InitializeBetting()
        {
            // 顯示初始資金
            this.txtMoney.Text = totalMoney.ToString("N0");
            // 發牌按鈕預設為禁用，需要先下注
            this.btnDealCard.Enabled = false;
            // 綁定下注按鈕的 Click 事件
            this.btnBet.Click += BtnBet_Click;
        }

        /// <summary>
        /// 顯示五張撲克牌到桌面上
        /// </summary>
        private void ShowCards()
        {
            for (int i = 0; i < playerPoker.Length; i++)
            {
                pic[i].Image = this.GetImage($"pic{playerPoker[i] + 1}");
            }
        }


        /// <summary>
        /// 取得圖片資源
        /// </summary>
        /// <param name="name">string 的牌名 </param>
        /// <returns></returns>
        private Image GetImage(string name)
        {
            return Properties.Resources.ResourceManager.GetObject(name) as Image;
        }

        /// <summary>
        /// 取得圖片資源
        /// </summary>
        /// <param name="num">撲克牌編號</param>
        /// <returns></returns>
        private Image GetImage(int num)
        {
            return GetImage($"pic{num}");
        }


        /// <summary>
        /// 將 allPoker 陣列中的牌隨機打亂，模擬洗牌的過程
        /// </summary>
        private void Shuffle()
        {
            Random rand = new Random();
            for (int i = 0; i < 1000; i++)
            {
                int r = rand.Next(allPoker.Length);
                int temp = allPoker[r];
                allPoker[r] = allPoker[0];
                allPoker[0] = temp;
            }
        }

        /// <summary>
        /// 判斷牌型並回傳牌型的英文代碼
        /// </summary>
        private string GetHandType(int[] pokerColor, int[] pokerPoint)
        {
            // 記錄花色和點數出現次數的陣列
            int[] colorCount = new int[4];
            int[] pointCount = new int[13];

            // 統計 color 和 point 出現次數
            for (int i = 0; i < pokerColor.Length; i++)
            {
                int color = pokerColor[i];
                int point = pokerPoint[i];

                colorCount[color]++;
                pointCount[point]++;
            }

            // 排序
            Array.Sort(colorCount);
            Array.Reverse(colorCount);

            Array.Sort(pointCount);
            Array.Reverse(pointCount);

            // 判斷是否為同花
            bool isFlush = (colorCount[0] == 5);
            // 判斷是否為五張單張
            bool isSingle = (pointCount[0] == 1 && pointCount[1] == 1 && pointCount[2] == 1 && pointCount[3] == 1 && pointCount[4] == 1);
            // 判斷是否為差四
            bool isDiffFout = (pokerPoint.Max() - pokerPoint.Min() == 4);
            // 判斷是否為大順
            bool isRoyal = pokerPoint.Contains(0) && pokerPoint.Contains(9) && pokerPoint.Contains(10) && pokerPoint.Contains(11) && pokerPoint.Contains(12);
            // 判斷是否為同花大順
            bool isRoyalisFlush = isFlush && isRoyal;
            // 判斷是否為同花順
            bool isStraightFlush = isFlush && isSingle && isDiffFout;
            // 判斷是否為順子
            bool isStraight = isSingle && (isDiffFout || isRoyal);
            // 判斷是否為鐵支
            bool isFourOfAKind = (pointCount[0] == 4);
            // 判斷是否為葫蘆
            bool isFullHouse = (pointCount[0] == 3 && pointCount[1] == 2);
            // 判斷是否為三條
            bool isThreeOfAKind = (pointCount[0] == 3 && pointCount[1] == 1);
            // 判斷是否為兩對
            bool isTwoPair = (pointCount[0] == 2 && pointCount[1] == 2);
            // 判斷是否為一對
            bool isOnePair = (pointCount[0] == 2 && pointCount[1] == 1);

            if (isRoyalisFlush)
                return "RoyalFlush";
            else if (isStraightFlush)
                return "StraightFlush";
            else if (isFourOfAKind)
                return "FourOfAKind";
            else if (isFullHouse)
                return "FullHouse";
            else if (isFlush)
                return "Flush";
            else if (isStraight)
                return "Straight";
            else if (isThreeOfAKind)
                return "ThreeOfAKind";
            else if (isTwoPair)
                return "TwoPair";
            else if (isOnePair)
                return "OnePair";
            else
                return "Nothing";
        }

        /// <summary>
        /// 取得牌型的中文名稱
        /// </summary>
        private string GetHandTypeName(string handType, int[] pokerColor, int[] pokerPoint, string[] colorList, string[] pointList)
        {
            // 重新排序以取得排序後的牌型資訊
            int[] colorCount = new int[4];
            int[] pointCount = new int[13];

            for (int i = 0; i < pokerColor.Length; i++)
            {
                colorCount[pokerColor[i]]++;
                pointCount[pokerPoint[i]]++;
            }

            string[] colorListCopy = (string[])colorList.Clone();
            string[] pointListCopy = (string[])pointList.Clone();

            Array.Sort(colorCount, colorListCopy);
            Array.Reverse(colorCount);
            Array.Reverse(colorListCopy);

            Array.Sort(pointCount, pointListCopy);
            Array.Reverse(pointCount);
            Array.Reverse(pointListCopy);

            switch (handType)
            {
                case "RoyalFlush":
                    return $"{colorListCopy[0]} 皇家同花順";
                case "StraightFlush":
                    return $"{colorListCopy[0]} 同花順";
                case "FourOfAKind":
                    return $"{pointListCopy[0]} 鐵支";
                case "FullHouse":
                    return $"{pointListCopy[0]}三張{pointListCopy[1]}兩張 葫蘆";
                case "Flush":
                    return $"{colorListCopy[0]} 同花";
                case "Straight":
                    return "順子";
                case "ThreeOfAKind":
                    return $"{pointListCopy[0]} 三條";
                case "TwoPair":
                    return $"{pointListCopy[0]},{pointListCopy[1]} 兩對";
                case "OnePair":
                    return $"{pointListCopy[0]} 一對";
                default:
                    return "雜牌";
            }
        }

        #endregion

           
        #region 事件處理程序

        /// <summary>
        /// 下注按鈕的 Click 事件
        /// </summary>
        private void BtnBet_Click(object sender, EventArgs e)
        {
            // 驗證下注金額是否為有效的數字
            if (!decimal.TryParse(this.txtBet.Text, out decimal bet))
            {
                MessageBox.Show("請輸入有效的下注金額", "輸入錯誤", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // 驗證下注金額是否大於 0
            if (bet <= 0)
            {
                MessageBox.Show("下注金額必須大於 0", "輸入錯誤", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // 驗證下注金額是否超過總資金
            if (bet > totalMoney)
            {
                MessageBox.Show($"下注金額不能超過總資金 {totalMoney:N0}", "輸入錯誤", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // 記錄下注金額
            betAmount = bet;
            hasBetThisRound = true;

            // 扣除已下注的金額
            totalMoney -= bet;
            this.txtMoney.Text = totalMoney.ToString("N0");

            // 清空下注輸入框
            this.txtBet.Text = "";

            // 禁用下注按鈕，啟用發牌按鈕（不清空下注金額）
            this.btnBet.Enabled = false;
            this.btnDealCard.Enabled = true;

            MessageBox.Show($"下注成功！本局下注金額：{bet:N0}", "下注確認", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        /// <summary>
        /// 牌桌上的牌被按下時，顯示訊息框告訴使用者按下了哪一張牌
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Pic_Click(object sender, EventArgs e)
        {
            PictureBox pic = sender as PictureBox;

            // 如果牌未被啟用（未發牌），禁止翻牌
            if (!pic.Enabled)
            {
                MessageBox.Show("請先發牌才能翻牌", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            int index = int.Parse(pic.Name.Replace("pic", ""));

            int cardNum = playerPoker[index] + 1;

            // 如果牌面朝下，則翻開牌面；如果牌面朝上，則翻回背面
            if (pic.Tag.ToString() == "back")
            {
                pic.Tag = "front";
                pic.Image = GetImage(cardNum);
            }
            else
            {
                pic.Tag = "back";
                pic.Image = GetImage("back");
            }
        }

        /// <summary>
        /// 當按下發牌按鈕時，隨機產生五個1~52的數字，並將對應的圖片顯示在牌桌上
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>

        private async void btnDealCard_Click(object sender, EventArgs e)
        {
            // 將上一把玩的結果清除
            this.lblResult.Text = "";


            // 將牌桌上的牌重置為背面圖
            for (int i = 0; i < pic.Length; i++)
            {
                pic[i].Image = GetImage("back");
            }

            // 將所有牌的編號從 0 到 51 填入 allPoker 陣列
            for (int i = 0; i < allPoker.Length; i++)
            {
                allPoker[i] = i;
            }

            // 洗牌
            this.Shuffle();

            // 暫停500ms
            await Task.Delay(500);

            // 發前五張牌給玩家，並將對應的牌面圖顯示在牌桌上
            for (int i = 0; i < playerPoker.Length; i++)
            {
                // 取前52張牌的前五張牌
                playerPoker[i] = allPoker[i];
            }


            //// 測試用
            //playerPoker[0] = 51;
            //playerPoker[1] = 47;
            //playerPoker[2] = 43;
            //playerPoker[3] = 39;
            //playerPoker[4] = 3;


            // 將對應的牌面圖顯示在牌桌上
            this.ShowCards();

            // 啟用所有牌的點擊事件
            for (int i = 0; i < pic.Length; i++)
            {
                // 將牌桌上的牌設成可以點擊
                pic[i].Enabled = true;
                // 將牌桌上的牌的 Tag 設成 "front"，表示牌面朝上
                pic[i].Tag = "front";
            }

            // 啟用換牌按鈕
            btnChangeCard.Enabled = true;
            btnDealCard.Enabled = false;

        }

        /// <summary>
        /// 當按下換牌按鈕時，將玩家手牌中被選中的牌換成新的牌，並將對應的圖片顯示在牌桌上
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnChangeCard_Click(object sender, EventArgs e)
        {
            int startIndex = 5; // 從 allPoker 陣列的第 5 張牌開始換牌，因為前 5 張牌已經發給玩家了

            for(int i = 0; i < playerPoker.Length; i++)
            {
                // 如果牌面朝下，表示玩家選擇換掉這張牌
                if (pic[i].Tag.ToString() == "back")
                {
                    // 將玩家手牌中被選中的牌換成新的牌
                    playerPoker[i] = allPoker[startIndex];
                    // 將對應的牌面圖顯示在牌桌上
                    pic[i].Image = GetImage(playerPoker[i] + 1);
                    pic[i].Tag = "front";

                    startIndex++;
                }
            }

            for(int i = 0; i < pic.Length; i++)
            {
                // 將牌桌上的牌設成不可點擊
                pic[i].Enabled = false;
            }

            // 將換牌按鈕設成不可用，表示玩家已經完成換牌了
            this.btnChangeCard.Enabled = false;

            // 將判斷牌型的按鈕設成可用，表示玩家可以開始判斷牌型了
            this.btnCheck.Enabled = true;
        }

        /// <summary>
        /// 當按下判斷牌型按鈕時，根據玩家手牌的編號，判斷玩家的牌型，並顯示在 lblResult 上
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnCheck_Click(object sender, EventArgs e)
        {
            string[] colorList = { "梅花", "方塊", "愛心", "黑桃" };
            string[] pointList = { "A", "2", "3", "4", "5", "6", "7", "8", "9", "10", "J", "Q", "K" };


            // 計錄目前五張撲克牌的花色的陣列
            int[] pokerColor = new int[5];
            // 計錄目前五張撲克牌的點數的陣列
            int[] pokerPoint = new int[5];


            for (int i = 0; i < playerPoker.Length; i++)
            {
                // 根據玩家手牌的編號，計算出玩家手牌的花色 
                pokerColor[i] = playerPoker[i] % 4;
                // 根據玩家手牌的編號，計算出玩家手牌的點數
                pokerPoint[i] = playerPoker[i] / 4;
            }

            // 判斷牌型
            string handType = GetHandType(pokerColor, pokerPoint);
            string handTypeName = GetHandTypeName(handType, pokerColor, pokerPoint, colorList, pointList);

            // 計算賠率和獲利
            decimal payout = 0;
            if (handType != "Nothing")
            {
                payout = betAmount * payoutOdds[handType];
                // 更新資金（加回下注金額 + 獲利）
                totalMoney += betAmount + payout;
            }
            // 如果是雜牌，下注金額已在下注時扣除，不需要再加回

            this.txtMoney.Text = totalMoney.ToString("N0");

            // 顯示結果
            string resultText = handTypeName;
            if (handType != "Nothing")
            {
                resultText += $"\n賠率：{payoutOdds[handType]}倍\n獲利：{payout:N0}";
            }
            else
            {
                resultText += "\n無獲利（下注金額已扣除）";
            }

            lblResult.Text = resultText;

            btnChangeCard.Enabled = false;
            btnCheck.Enabled = false;

            // 重置下注狀態，準備下一局
            ResetBettingForNextRound();
        }

        /// <summary>
        /// 重置下注狀態，為下一局做準備
        /// </summary>
        private void ResetBettingForNextRound()
        {
            hasBetThisRound = false;
            betAmount = 0;
            // 不清空下注輸入框
            this.btnBet.Enabled = true;
            this.btnDealCard.Enabled = false;
        }

        /// <summary>
        /// 當表單被按下鍵盤時觸發
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void frmPoker_KeyPress(object sender, KeyPressEventArgs e)
        {
            // 只有在發牌後且未發牌時才允許使用快捷鍵（測試模式）
            if (this.btnDealCard.Enabled == false && this.btnBet.Enabled == false)
            {
                switch(e.KeyChar)
                {
                    case 'q':
                        // 同花大順
                        playerPoker[0] = 51;
                        playerPoker[1] = 47;
                        playerPoker[2] = 43;
                        playerPoker[3] = 39;
                        playerPoker[4] = 3;
                        this.ShowCards();
                        e.Handled = true;
                        break;
                    case 'w':
                        // 同花順
                        playerPoker[0] = 37;
                        playerPoker[1] = 33;
                        playerPoker[2] = 29;
                        playerPoker[3] = 25;
                        playerPoker[4] = 21;
                        this.ShowCards();
                        e.Handled = true;
                        break;
                    case 'e':
                        // 同花
                        playerPoker[0] = 50;
                        playerPoker[1] = 38;
                        playerPoker[2] = 34;
                        playerPoker[3] = 22;
                        playerPoker[4] = 18;
                        this.ShowCards();
                        e.Handled = true;
                        break;
                    case 'r':
                        // 鐵支
                        playerPoker[0] = 48;
                        playerPoker[1] = 39;
                        playerPoker[2] = 38;
                        playerPoker[3] = 37;
                        playerPoker[4] = 36;
                        this.ShowCards();
                        e.Handled = true;
                        break;
                    case 't':
                        // 葫蘆
                        playerPoker[0] = 30;
                        playerPoker[1] = 29;
                        playerPoker[2] = 6;
                        playerPoker[3] = 5;
                        playerPoker[4] = 4;
                        this.ShowCards();
                        e.Handled = true;
                        break;
                    case 'y':
                        // 三條
                        playerPoker[0] = 48;
                        playerPoker[1] = 39;
                        playerPoker[2] = 15;
                        playerPoker[3] = 14;
                        playerPoker[4] = 13;
                        this.ShowCards();
                        e.Handled = true;
                        break;
                }
            }
        }
        #endregion


    }
}
