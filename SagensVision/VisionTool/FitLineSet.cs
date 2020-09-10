using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Linq;
using System.Windows.Forms;
using DevExpress.XtraEditors;
using System.Text.RegularExpressions;
using HalconDotNet;
using ChoiceTech.Halcon.Control;
using SagensSdk;
using System.IO;
using ViewWindow.Model;
using System.Collections;
using System.Diagnostics;
using System.Drawing.Imaging;
using System.Globalization;

namespace SagensVision.VisionTool
{
    public partial class FitLineSet : DevExpress.XtraEditors.XtraForm
    {
               
        int CurrentIndex = 0;
        HWindow_Final hwindow_final1 = new HWindow_Final();
        HWindow_Final hwindow_final2 = new HWindow_Final();
        HWindow_Final hwindow_final3 = new HWindow_Final();
        //public FitProfileParam[] fpTool.fParam = new FitProfileParam[4];
        ROIController roiController;
        ROIController roiController2;
        ROIController roiController3;

        //List<ROI>[] fpTool.roiList = new List<ROI>[4];
        //List<ROI>[] fpTool.fpTool.roiList2 = new List<ROI>[4];
        List<ROI> tempList = new List<ROI>();
        //List<ROI>[] fpTool.roiList3 = new List<ROI>[4];//校准区域
        HObject HeightImage = new HObject();
        HObject IntensityImage = new HObject();
        HObject RGBImage = new HObject();
        HObject OriginImage = new HObject();

        HObject GlueHeightImage = new HObject();


        //public IntersetionCoord[] intersectCoordList = new IntersetionCoord[4];
        //计算输出交点
        IntersetionCoord intersection = new IntersetionCoord();
        public FindPointTool fpTool = new FindPointTool();
        bool isRight = true;
        ProfileResult pResult = new ProfileResult();

        List<HTuple> _lines = new List<HTuple>();
        public List<HTuple> H_Lines
        {
            get
            {
                return _lines;
            }
        }

        /// <summary>
        /// "工具类型 Calib  or GlueGuide"
        /// </summary>
        public string ToolType = "";
        /// <summary>
        /// 找线方式 "Fix" or "FitLineSet"
        /// </summary>
        public string FindType = "";
        public FitLineSet(string text = "")
        {
            InitializeComponent();
            if (text != "")
            {
                this.Text = text;
            }

        }

        private void FitLineSet_Load(object sender, EventArgs e)
        {
            if (this.Text == "Detect")
            {
                xtraTabPage2.PageVisible = false;
                InitDgv3();
            }
            RoiParam.isInvoke = false;
            isLoading = true;
            isRight = MyGlobal.IsRight;
            //string ok = fpTool.Init(FindType, isRight, ToolType);
            //if (ok != "OK")
            //{
            //    MessageBox.Show(ok);
            //}
            this.MaximizeBox = true;
            CurrentSide = "Side1";
            isSave = true;
            isCloing = false;
            splitContainerControl4.Panel1.Controls.Add(hwindow_final2);
            if (this.Text != "Detect")
            {
                splitContainerControl6.Panel1.Controls.Add(hwindow_final1);
            }
            else
            {
                DevExpress.XtraEditors.SplitContainerControl splitContainerControl7 = new DevExpress.XtraEditors.SplitContainerControl();
                splitContainerControl6.Panel1.Controls.Add(splitContainerControl7);
                ((System.ComponentModel.ISupportInitialize)(splitContainerControl7)).BeginInit();
                splitContainerControl7.SuspendLayout();
                splitContainerControl7.Dock = System.Windows.Forms.DockStyle.Fill;
                splitContainerControl7.Horizontal = true;
                splitContainerControl7.Size = new System.Drawing.Size(968, 622);
                splitContainerControl7.SplitterPosition = 484;
                ((System.ComponentModel.ISupportInitialize)(splitContainerControl7)).EndInit();
                splitContainerControl7.ResumeLayout(false);
                splitContainerControl7.Panel1.Controls.Add(hwindow_final1);
                splitContainerControl7.Panel2.Controls.Add(hwindow_final3);
                roiController3 = hwindow_final3.viewWindow._roiController;
                roiController3.NotifyRCObserver = new IconicDelegate(ROiMove3);
            }

            //splitContainerControl1.Panel1.Controls.Add(hwindow_final3);


            hwindow_final1.Dock = DockStyle.Fill;
            hwindow_final2.Dock = DockStyle.Fill;
            hwindow_final3.Dock = DockStyle.Fill;
            trackBarControl1.Properties.Minimum = 1;

            //LoadToUI(0);
            roiController = hwindow_final1.viewWindow._roiController;
            roiController.NotifyRCObserver = new IconicDelegate(ROiMove);
            roiController2 = hwindow_final2.viewWindow._roiController;
            roiController2.NotifyRCObserver = new IconicDelegate(ROiMove2);
            comboBox2.SelectedIndex = 0;
            comboBox1.SelectedIndex = 0;

            CurrentSide = "Side1";
            SideName = CurrentSide;
            hwindow_final1.viewWindow.setEditModel(true);
            hwindow_final2.viewWindow.setEditModel(true);
            checkBoxRoi.Checked = true;
            ChangeSide();
            hwindow_final2.hWindowControl.MouseDown += Hwindow_final2_MouseDown;
            isLoading = false;
            RoiParam.isInvoke = true;
            RoiParam.ChangeSection += RoiParam_ChangeSection;//改变参数设置事件
            isSave = true;
            
            this.dataGridView3.CellValueChanged += dataGridView3_CellValueChanged;
        }

        private void ROiMove3(int value)
        {
            try
            {
                switch (value)
                {
                    case ROIController.EVENT_UPDATE_ROI:
                        isSave = false;
                        int Id = Convert.ToInt32(SideName.Substring(4, 1));
                        if (dataGridView1.CurrentCell == null)
                        {
                            return;
                        }
                        int RowId = dataGridView1.CurrentCell.RowIndex;
                        RowId = CurrentRowIndex;

                        if (((RowId < 0)))
                        {
                            return;
                        }
                        fpTool.DetectRoiList[Id - 1][RowId] = temp_Detect[0];


                        //记录当前锚定点坐标

                        HTuple row, col;
                        fpTool.FindFirstAnchor(Id, out row, out col, CurrentIndex);
                        if (row.Length == 0)
                        {
                            return;
                        }
                        fpTool.fParam[Id - 1].roiP[RowId].AnchorRow = row.D;
                        fpTool.fParam[Id - 1].roiP[RowId].AnchorCol = col.D;

                        ROI pt = roiController3.getActiveROI();
                        if (pt.Type == "ROIPoint")
                        {
                            HTuple PointTemp = pt.getModelData(); HTuple row1, col1;
                            fpTool.FindMaxPt(Id, CurrentIndex - 1, out row1, out col1, out row, out col, null, ShowSection, false, null);
                            switch (PtOrder)
                            {
                                case 0:

                                    fpTool.fParam[Id - 1].roiP[RowId].StartOffSet1.Y = (int)(PointTemp[0].D - row.D);
                                    fpTool.fParam[Id - 1].roiP[RowId].StartOffSet1.X = (int)(PointTemp[1].D - col.D);
                                    PtOrder = -1;
                                    break;
                                case 1:

                                    fpTool.fParam[Id - 1].roiP[RowId].EndOffSet1.Y = (int)(PointTemp[0].D - row.D);
                                    fpTool.fParam[Id - 1].roiP[RowId].EndOffSet1.X = (int)(PointTemp[1].D - col.D);
                                    PtOrder = -1;
                                    break;
                                case 2:

                                    fpTool.fParam[Id - 1].roiP[RowId].StartOffSet2.Y = (int)(PointTemp[0].D - row.D);
                                    fpTool.fParam[Id - 1].roiP[RowId].StartOffSet2.X = (int)(PointTemp[1].D - col.D);
                                    PtOrder = -1;
                                    break;
                                case 3:

                                    fpTool.fParam[Id - 1].roiP[RowId].EndOffSet2.Y = (int)(PointTemp[0].D - row.D);
                                    fpTool.fParam[Id - 1].roiP[RowId].EndOffSet2.X = (int)(PointTemp[1].D - col.D);
                                    PtOrder = -1;
                                    break;
                            }

                        }

                        break;
                    default:
                        break;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("RoiMove3-->" + ex.Message);
            }
        }

        private void RoiParam_ChangeSection(ValueChangedType obj, double value)
        {
            try
            {

                if (isLoading || RoiIsMoving)
                {
                    return;
                }
                if (RoiParam.isInvoke)
                {
                    RoiParam.isInvoke = false;
                }
                isSave = false;
                int SideId = Convert.ToInt32(SideName.Substring(4, 1)) - 1;
                int roiID = dataGridView1.CurrentCell.RowIndex;
                roiID = CurrentRowIndex;
                int count = dataGridView1.SelectedCells.Count;
                List<int> rowInd = new List<int>();
                for (int i = 0; i < count; i++)
                {
                    int Ind = dataGridView1.SelectedCells[i].RowIndex;
                    if (!rowInd.Contains(Ind))
                    {
                        rowInd.Add(Ind);
                    }
                }
                //for (int i = 0; i < rowInd.Count; i++)
                //{
                //    roiID = rowInd[i];
                //    HTuple[] lineCoord = new HTuple[1];
                //}

                bool roiUpdate = false;
                for (int i = 0; i < rowInd.Count; i++)
                {
                    roiID = rowInd[i];
                    HTuple[] lineCoord = new HTuple[1];

                    switch (obj)
                    {
                        case ValueChangedType.数量:
                            fpTool.fParam[SideId].roiP[roiID].NumOfSection = (int)value;
                            fpTool.DispSection((ROIRectangle2)fpTool.roiList2[SideId][roiID], SideId, roiID, out lineCoord, hwindow_final2);
                            roiUpdate = true;
                            break;
                        case ValueChangedType.长度:
                            HTuple temp = new HTuple();
                            List<ROI> temproi = new List<ROI>();
                            temp = fpTool.roiList2[SideId][roiID].getModelData();
                            fpTool.fParam[SideId].roiP[roiID].Len1 = value;
                            hwindow_final2.viewWindow.genRect2(fpTool.fParam[SideId].roiP[roiID].CenterRow, fpTool.fParam[SideId].roiP[roiID].CenterCol, fpTool.fParam[SideId].roiP[roiID].phi, fpTool.fParam[SideId].roiP[roiID].Len1, fpTool.fParam[SideId].roiP[roiID].Len2, ref temproi);
                            fpTool.roiList2[SideId][roiID] = temproi[0];
                            roiUpdate = true;
                            break;
                        case ValueChangedType.宽度:
                            HTuple temp3 = new HTuple();
                            temp3 = fpTool.roiList2[SideId][roiID].getModelData();
                            List<ROI> temproi3 = new List<ROI>();
                            fpTool.fParam[SideId].roiP[roiID].Len2 = value;
                            hwindow_final2.viewWindow.genRect2(fpTool.fParam[SideId].roiP[roiID].CenterRow, fpTool.fParam[SideId].roiP[roiID].CenterCol, fpTool.fParam[SideId].roiP[roiID].phi, fpTool.fParam[SideId].roiP[roiID].Len1, fpTool.fParam[SideId].roiP[roiID].Len2, ref temproi3);
                            fpTool.roiList2[SideId][roiID] = temproi3[0];
                            roiUpdate = true;
                            break;
                        case ValueChangedType.角度:
                            HTuple temp6 = new HTuple();
                            temp3 = fpTool.roiList2[SideId][roiID].getModelData();
                            List<ROI> temproi6 = new List<ROI>();
                            fpTool.fParam[SideId].roiP[roiID].phi = value;
                            hwindow_final2.viewWindow.genRect2(fpTool.fParam[SideId].roiP[roiID].CenterRow, fpTool.fParam[SideId].roiP[roiID].CenterCol, fpTool.fParam[SideId].roiP[roiID].phi, fpTool.fParam[SideId].roiP[roiID].Len1, fpTool.fParam[SideId].roiP[roiID].Len2, ref temproi6);
                            fpTool.roiList2[SideId][roiID] = temproi6[0];
                            roiUpdate = true;
                            break;
                        case ValueChangedType.行坐标:
                            HTuple temp4 = new HTuple();
                            temp4 = fpTool.roiList2[SideId][roiID].getModelData();
                            List<ROI> temproi4 = new List<ROI>();
                            fpTool.fParam[SideId].roiP[roiID].CenterRow = value;
                            hwindow_final2.viewWindow.genRect2(fpTool.fParam[SideId].roiP[roiID].CenterRow, fpTool.fParam[SideId].roiP[roiID].CenterCol, fpTool.fParam[SideId].roiP[roiID].phi, fpTool.fParam[SideId].roiP[roiID].Len1, fpTool.fParam[SideId].roiP[roiID].Len2, ref temproi4);
                            fpTool.roiList2[SideId][roiID] = temproi4[0];
                            roiUpdate = true;
                            break;
                        case ValueChangedType.列坐标:
                            HTuple temp5 = new HTuple();
                            temp3 = fpTool.roiList2[SideId][roiID].getModelData();
                            List<ROI> temproi5 = new List<ROI>();
                            fpTool.fParam[SideId].roiP[roiID].CenterCol = value;
                            hwindow_final2.viewWindow.genRect2(fpTool.fParam[SideId].roiP[roiID].CenterRow, fpTool.fParam[SideId].roiP[roiID].CenterCol, fpTool.fParam[SideId].roiP[roiID].phi, fpTool.fParam[SideId].roiP[roiID].Len1, fpTool.fParam[SideId].roiP[roiID].Len2, ref temproi5);
                            fpTool.roiList2[SideId][roiID] = temproi5[0];
                            roiUpdate = true;
                            break;
                        case ValueChangedType.Roi方向偏移:
                            fpTool.fParam[SideId].roiP[roiID].offset = value;
                            break;
                        case ValueChangedType.X方向偏移:
                            fpTool.fParam[SideId].roiP[roiID].Xoffset = value;
                            break;
                        case ValueChangedType.Y方向偏移:
                            fpTool.fParam[SideId].roiP[roiID].Yoffset = value;
                            break;
                        case ValueChangedType.Z方向偏移:
                            fpTool.fParam[SideId].roiP[roiID].Zoffset = value;
                            break;
                        case ValueChangedType.X方向距离:
                            fpTool.fParam[SideId].roiP[roiID].xDist = value;
                            break;
                        case ValueChangedType.取最近点还是最远点:
                            fpTool.fParam[SideId].roiP[roiID].useNear = value ==1 ? true : false;
                            break;
                        case ValueChangedType.取轮廓区域中心点:
                            fpTool.fParam[SideId].roiP[roiID].useCenter = value == 1 ? true : false;
                            break;
                        case ValueChangedType.是否取中间点:
                            fpTool.fParam[SideId].roiP[roiID].useMidPt = value == 1 ? true : false;
                            break;
                        case ValueChangedType.是否取左侧值:
                            fpTool.fParam[SideId].roiP[roiID].useLeft = value == 1 ? true : false;
                            break;
                        case ValueChangedType.是否启用Z向缩放:
                            fpTool.fParam[SideId].roiP[roiID].useZzoom = value == 1 ? true : false;
                            break;
                        case ValueChangedType.找线方式设置:
                            fpTool.fParam[SideId].roiP[roiID].TypeOfFindLine = value == 0 ? "极值" : "最大值";
                            break;
                        case ValueChangedType.最高点下降距离:
                            fpTool.fParam[SideId].roiP[roiID].TopDownDist = value ;
                            break;
                        case ValueChangedType.直线滤波系数:
                            fpTool.fParam[SideId].roiP[roiID].SmoothCont = value ;
                            break;
                        case ValueChangedType.轮廓Z向拉伸:
                            fpTool.fParam[SideId].roiP[roiID].ClippingPer = value;
                            break;
                        case ValueChangedType.高度方向滤波最大百分比:
                            fpTool.fParam[SideId].roiP[roiID].ZftMax = value;
                            break;
                        case ValueChangedType.高度方向滤波最小百分比:
                            fpTool.fParam[SideId].roiP[roiID].ZftMin = value;
                            break;
                        case ValueChangedType.高度方向滤波半径:
                            fpTool.fParam[SideId].roiP[roiID].ZftRad = value;
                            break;
                        case ValueChangedType.轮廓平滑:
                            fpTool.fParam[SideId].roiP[roiID].Sigma = value;
                            break;
                        case ValueChangedType.轮廓旋转角度:
                            fpTool.fParam[SideId].roiP[roiID].AngleOfProfile = (int) value;
                            break;
                        default:
                            break;
                    }
                }
                if (roiUpdate)
                {
                    hwindow_final2.viewWindow.notDisplayRoi();
                    hwindow_final2.viewWindow.displayROI(ref fpTool.roiList2[SideId]);
                    hwindow_final2.viewWindow.selectROI(roiID);
                }
                


                RoiParam.isInvoke = true;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void Hwindow_final2_MouseDown(object sender, MouseEventArgs e)
        {
            int Id = Convert.ToInt32(SideName.Substring(4, 1)) - 1;
            ArrayList array = roiController2.ROIList;
            int currentId = -1; string Name = "";
            if (dataGridView1.CurrentCell == null || CurrentRowIndex == -1)
            {
                currentId = 0;
            }
            else
            {
                currentId = dataGridView1.CurrentCell.RowIndex;
                currentId = CurrentRowIndex;
                //ID
                Name = dataGridView1.Rows[currentId].Cells[1].Value.ToString();
            }
            int ActiveId = roiController2.getActiveROIIdx();
            if (!SelectAll)
            {
                if (ActiveId != currentId)
                {
                    roiController2.EditModel = false;
                }
                else
                {
                    roiController2.EditModel = true;

                }
            }
        }

        public void ROiMove(int value)
        {
            try
            {
                switch (value)
                {
                    //case ROIController.EVENT_CHANGED_ROI_SIGN:
                    //case ROIController.EVENT_DELETED_ACTROI:
                    //case ROIController.EVENT_DELETED_ALL_ROIS:
                    case ROIController.EVENT_UPDATE_ROI:

                        isSave = false;

                        int Id = Convert.ToInt32(SideName.Substring(4, 1));
                        if (dataGridView1.CurrentCell == null)
                        {
                            return;
                        }
                        int RowId = dataGridView1.CurrentCell.RowIndex;
                        RowId = CurrentRowIndex;

                        if (((RowId < 0 || FindPointTool.RArray == null) && this.Text != "Detect"))
                        {
                            return;
                        }
                        fpTool.roiList[Id - 1][RowId] = temp[0];


                        //记录当前锚定点坐标

                        HTuple row, col;
                        fpTool.FindFirstAnchor(Id, out row, out col, CurrentIndex);
                        if (row.Length==0)
                        {
                            return;
                        }
                        fpTool.fParam[Id - 1].roiP[RowId].AnchorRow = row.D;
                        fpTool.fParam[Id - 1].roiP[RowId].AnchorCol = col.D;

                        ROI pt = roiController.getActiveROI();
                        if (pt.Type == "ROIPoint")
                        {
                            HTuple PointTemp = pt.getModelData(); HTuple row1, col1;
                            fpTool.FindMaxPt(Id, CurrentIndex - 1, out row1, out col1, out row, out col, null, ShowSection, false, null);
                            switch (PtOrder)
                            {
                                case 0:

                                    fpTool.fParam[Id - 1].roiP[RowId].StartOffSet1.Y = (int)(PointTemp[0].D - row.D);
                                    fpTool.fParam[Id - 1].roiP[RowId].StartOffSet1.X = (int)(PointTemp[1].D - col.D);
                                    PtOrder = -1;
                                    break;
                                case 1:

                                    fpTool.fParam[Id - 1].roiP[RowId].EndOffSet1.Y = (int)(PointTemp[0].D - row.D);
                                    fpTool.fParam[Id - 1].roiP[RowId].EndOffSet1.X = (int)(PointTemp[1].D - col.D);
                                    PtOrder = -1;
                                    break;
                                case 2:

                                    fpTool.fParam[Id - 1].roiP[RowId].StartOffSet2.Y = (int)(PointTemp[0].D - row.D);
                                    fpTool.fParam[Id - 1].roiP[RowId].StartOffSet2.X = (int)(PointTemp[1].D - col.D);
                                    PtOrder = -1;
                                    break;
                                case 3:

                                    fpTool.fParam[Id - 1].roiP[RowId].EndOffSet2.Y = (int)(PointTemp[0].D - row.D);
                                    fpTool.fParam[Id - 1].roiP[RowId].EndOffSet2.X = (int)(PointTemp[1].D - col.D);
                                    PtOrder = -1;
                                    break;
                            }

                        }

                        break;
                    //case HWndCtrl.ERR_READING_IMG:
                    //    MessageBox.Show("Problem occured while reading file! \n", "Profile ",
                    //        MessageBoxButtons.OK, MessageBoxIcon.Information);
                    //    break;
                    default:
                        break;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("RoiMove-->" + ex.Message);
            }
        }
        bool RoiIsMoving = false;
        public void ROiMove2(int value)
        {
            try
            {
                switch (value)
                {
                    //case ROIController.EVENT_CHANGED_ROI_SIGN:
                    //case ROIController.EVENT_DELETED_ACTROI:
                    //case ROIController.EVENT_DELETED_ALL_ROIS:
                    case ROIController.EVENT_UPDATE_ROI:
                        if (isLoading)
                        {
                            return;
                        }
                        RoiIsMoving = true;
                        int Id = Convert.ToInt32(SideName.Substring(4, 1)) - 1;
                        ArrayList array = roiController2.ROIList;
                        int currentId = -1; string Name = "";
                        if (dataGridView1.CurrentCell == null || CurrentRowIndex == -1)
                        {
                            currentId = 0;
                        }
                        else
                        {
                            //currentId = dataGridView1.CurrentCell.RowIndex;
                            currentId = CurrentRowIndex;
                            //ID
                            Name = dataGridView1.Rows[currentId].Cells[1].Value.ToString();
                        }

                        if (fpTool.roiList2[Id].Count != array.Count)
                        {

                            fpTool.roiList2[Id].Add(new ROIRectangle2());

                            //每个区域单独设置Roi
                            hwindow_final1.viewWindow.notDisplayRoi();
                            hwindow_final1.viewWindow.genRect1(400, 400, 600, 600, ref fpTool.roiList[Id]);
                            hwindow_final3.viewWindow.notDisplayRoi();
                            hwindow_final3.viewWindow.genRect1(400, 400, 600, 600, ref fpTool.DetectRoiList[Id]);
                        }



                        int ActiveId = roiController2.getActiveROIIdx();
                        if (!SelectAll && !isGenSection)
                        {
                            if (ActiveId != currentId)
                            {
                                currentId = ActiveId;
                                //RoiIsMoving = false;
                                //return;
                            }
                        }

                        if (array.Count == 1)
                        {
                            ROI te = (ROI)array[0];
                            if (te.Type != "ROIRectangle2")
                            {
                                return;
                            }
                        }
                        if (fpTool.roiList2[Id].Count == 0)
                        {
                            return;
                        }

                       
                        fpTool.roiList2[Id][ActiveId] = (ROI)array[ActiveId];
                        
                        string key = "L";
                        if (fpTool.fParam[Id].roiP.Count != array.Count)
                        {
                            RoiParam RP = new RoiParam();

                            //RP.AngleOfProfile = Convert.ToInt32(textBox_Deg.Text);
                            //RP.NumOfSection = Convert.ToInt32(textBox_Num.Text);

                            fpTool.fParam[Id].roiP.Add(RP);
                            fpTool.fParam[Id].roiP[ActiveId].LineOrCircle = comboBox2.SelectedItem.ToString();

                            switch (fpTool.fParam[Id].roiP[ActiveId].LineOrCircle)
                            {
                                case "连接段":
                                    key = "LC";
                                    break;
                                case "直线段":
                                    key = "L";
                                    break;
                                case "圆弧段":
                                    key = "C";
                                    break;
                            }
                            key = key + (ActiveId + 1).ToString();
                            if (isGenSection)
                            {
                                isGenSection = false;
                                AddToDataGrid(key, fpTool.fParam[Id].roiP[ActiveId].LineOrCircle);
                                fpTool.fParam[Id].DicPointName.Add(key);
                            }

                            //操作DicPointName    

                        }
                        HTuple[] lineCoord = new HTuple[1];

                        

                        //listBox1.SelectedItem = ActiveId;
                        HTuple ModelData = fpTool.roiList2[Id][ActiveId].getModelData();
                        fpTool.fParam[Id].roiP[ActiveId].Len1 = ModelData[3];
                        fpTool.fParam[Id].roiP[ActiveId].Len2 = ModelData[4];
                        fpTool.fParam[Id].roiP[ActiveId].CenterRow = ModelData[0];
                        fpTool.fParam[Id].roiP[ActiveId].CenterCol = ModelData[1];
                        
                        //HTuple phi = ModelData[2];
                        fpTool.fParam[Id].roiP[ActiveId].phi = ModelData[2];
                        propertyGrid1.SelectedObject = fpTool.fParam[Id].roiP[ActiveId];
                        if (this.Text == "Detect")
                        {
                            ChangeDetectParam(Id, ActiveId);
                        }
                        //textBox_Len.Text = ((int)fpTool.fParam[Id].roiP[ActiveId].Len1).ToString();
                        //textBox_Width.Text = ((int)fpTool.fParam[Id].roiP[ActiveId].Len2).ToString();
                        //textBox_Row.Text = ((int)fpTool.fParam[Id].roiP[ActiveId].CenterRow).ToString();
                        //textBox_Col.Text = ((int)fpTool.fParam[Id].roiP[ActiveId].CenterCol).ToString();
                        //HTuple deg = new HTuple();
                        //HOperatorSet.TupleDeg(ModelData[2], out deg);
                        //textBox_phi.Text = ((int)deg.D).ToString();


                        PreSelect = Name;


                        //刷新界面roi

                        //

                        if (fpTool.roiList2[Id].Count > 0)
                        {
                            if (SelectAll)
                            {
                                hwindow_final2.viewWindow.notDisplayRoi();
                                roiController2.viewController.ShowAllRoiModel = -1;
                                hwindow_final2.viewWindow.displayROI(ref fpTool.roiList2[Id]);
                            }
                            else
                            {
                                roiController2.viewController.ShowAllRoiModel = ActiveId;
                                roiController2.viewController.repaint(ActiveId);
                            }
                        }
                        hwindow_final2.viewWindow.selectROI(ActiveId);

                        dataGridView1.ClearSelection();
                        dataGridView1.Rows[ActiveId].Selected = true;
                        CurrentRowIndex = ActiveId;
                        //记录当前锚定点坐标
                        RoiIsMoving = false;
                        isSave = false;

                        break;
                    //case HWndCtrl.ERR_READING_IMG:
                    //    MessageBox.Show("Problem occured while reading file! \n", "Profile ",
                    //        MessageBoxButtons.OK, MessageBoxIcon.Information);
                    //    break;
                    default:
                        break;
                }
            }
            catch (Exception ex)
            {
                isInsert = false;
                MessageBox.Show(ex.Message);
            }
        }


        bool isLoading = false;
        void LoadToUI(int Index = 0)
        {
            try
            {
                RoiIsMoving = true;
                //isLoading = true;
                //刷新Roi参数
                UpdateRoiParam();
                ParamPath.ToolType = ToolType;
                ParamPath.ParaName = FindType + "_" + comboBox1.SelectedItem.ToString();
                ParamPath.IsRight = isRight;
                if (isRight)
                {
                    comboBox3.SelectedIndex = 0;
                    comboBox3.BackColor = Color.LimeGreen;
                }
                else
                {
                    comboBox3.SelectedIndex = 1;
                    comboBox3.BackColor = Color.Yellow;
                }

                string[] keys = fpTool.fParam[Index].DicPointName.ToArray();

                dataGridView1.Rows.Clear();
                //cb_x1.Items.Clear();
                //cb_y1.Items.Clear();
                for (int i = 0; i < keys.Length; i++)
                {
                    AddToDataGrid(keys[i], fpTool.fParam[Index].roiP[i].LineOrCircle);
                    //添加到锚定设置
                    if (FindType== MyGlobal.FindPointType_Fix)
                    {
                        //cb_x1.Items.Add(keys[i]);
                        //cb_y1.Items.Add(keys[i]);
                    }
                }

                if (keys.Length > 0)
                {
                    propertyGrid1.SelectedObject = fpTool.fParam[Index].roiP[0];

                    //textBox_Num.Text = ((int)fpTool.fParam[Index].roiP[0].NumOfSection).ToString();
                    //textBox_Len.Text = ((int)fpTool.fParam[Index].roiP[0].Len1).ToString();
                    //textBox_Width.Text = ((int)fpTool.fParam[Index].roiP[0].Len2).ToString();
                    //textBox_Row.Text = ((int)fpTool.fParam[Index].roiP[0].CenterRow).ToString();
                    //textBox_Col.Text = ((int)fpTool.fParam[Index].roiP[0].CenterCol).ToString();
                    //textBox_Offset.Text = ((int)fpTool.fParam[Index].roiP[0].offset).ToString();
                    //textBox_OffsetX.Text = ((int)fpTool.fParam[Index].roiP[0].Xoffset).ToString();
                    //textBox_OffsetY.Text = ((int)fpTool.fParam[Index].roiP[0].Yoffset).ToString();
                    //textBox_OffsetZ.Text = ((int)fpTool.fParam[Index].roiP[0].Zoffset).ToString();
                    //textBox_ZFtMax.Text = (fpTool.fParam[Index].roiP[0].ZftMax).ToString();
                    //textBox_ZFtMin.Text = (fpTool.fParam[Index].roiP[0].ZftMin).ToString();
                    //textBox_ZFtRad.Text = (fpTool.fParam[Index].roiP[0].ZftRad).ToString();

                    //HTuple deg = new HTuple();
                    //HOperatorSet.TupleDeg(fpTool.fParam[Index].roiP[0].phi, out deg);
                    //textBox_phi.Text = ((int)deg.D).ToString();
                    //textBox_Deg.Text = fpTool.fParam[Index].roiP[0].AngleOfProfile.ToString();
                    //checkBox_useLeft.Checked = fpTool.fParam[Index].roiP[0].useLeft;
                    //checkBox_midPt.Checked = fpTool.fParam[Index].roiP[0].useMidPt;
                    //checkBox_Far.Checked = !fpTool.fParam[Index].roiP[0].useNear;
                    //checkBox_center.Checked = fpTool.fParam[Index].roiP[0].useCenter;
                    //checkBox_zZoom.Checked = fpTool.fParam[Index].roiP[0].useZzoom;

                    //textBox_downDist.Text = fpTool.fParam[Index].roiP[0].TopDownDist.ToString();
                    //textBox_xDist.Text = fpTool.fParam[Index].roiP[0].xDist.ToString();
                    //textBox_Clipping.Text = fpTool.fParam[Index].roiP[0].ClippingPer.ToString();
                    //textBox_SmoothCont.Text = fpTool.fParam[Index].roiP[0].SmoothCont.ToString();
                    //comboBox_GetPtType.SelectedIndex = 0;
                }
                if (this.Text == "Detect")
                {
                    ChangeDetectParam(Index, 0);
                }
                RoiIsMoving = false;
                //isLoading = false;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.StackTrace);
            }
        }

        void AddToDataGrid(string ID, string Type)
        {

            dataGridView1.Rows.Add();
            int count = dataGridView1.Rows.Count;
            dataGridView1.Rows[count - 1].Cells[0].Value = count;
            dataGridView1.Rows[count - 1].Cells[1].Value = ID;
            dataGridView1.Rows[count - 1].Cells[2].Value = Type;
        }

        private void trackBarControl1_ValueChanged(object sender, EventArgs e)
        {
            CurrentIndex = trackBarControl1.Value;
            textBox_Current.Text = CurrentIndex.ToString();
            int Id = Convert.ToInt32(SideName.Substring(4, 1));
            HTuple row, col; HTuple anchor, anchorc;
            if (FindPointTool.RArray != null && FindPointTool.RArray.GetLength(0) > 0)
            {

                //button11_Click(sender, e);
                
                int roiID = -1;
                for (int i = 0; i < fpTool.fParam[Id - 1].roiP.Count; i++)
                {

                    for (int j = 0; j < fpTool.fParam[Id - 1].roiP[i].NumOfSection; j++)
                    {
                        roiID++;
                        if (roiID == CurrentIndex - 1)
                        {
                            break;
                        }
                    }
                    if (roiID == CurrentIndex - 1)
                    {
                        roiID = i;
                        break;
                    }

                }
                


                if (fpTool.fParam[Id - 1].roiP[roiID].SelectedType == 0)
                {

                    if (fpTool.fParam[Id - 1].roiP[roiID].TopDownDist != 0 && fpTool.fParam[Id - 1].roiP[roiID].xDist != 0)
                    {
                        //极值点下降
                        fpTool.FindMaxPtFallDown(Id, CurrentIndex - 1, out row, out col, out anchor, out anchorc, hwindow_final1, ShowSection, false, hwindow_final2);
                    }
                    else
                    {
                        fpTool.FindMaxPt(Id, CurrentIndex - 1, out row, out col, out anchor, out anchorc, hwindow_final2, ShowSection, false, hwindow_final1);
                    }
                }
                else
                {

                    fpTool.FindMaxPtFallDown(Id, CurrentIndex - 1, out row, out col, out anchor, out anchorc, hwindow_final1, ShowSection, false, hwindow_final2);
                }
            }
        }


        private void textBox_Current_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode != Keys.Enter)
            {
                return;
            }
            string index = textBox_Current.Text.ToString();
            bool ok = Regex.IsMatch(index, @"(?i)^[0-9]+$");
            if (CurrentIndex == 0 || index == "0")
            {
                return;
            }
            if (ok)
            {
                int num = int.Parse(index);
                CurrentIndex = num;
                trackBarControl1.Value = CurrentIndex;

            }
        }

        private void btn_open_Click(object sender, EventArgs e)
        {
            MyGlobal.GoSDK.EnableProfle = true;
            string error = "";
            if (!MyGlobal.GoSDK.IsConnected(ref error))
            {
                MessageBox.Show("请先连接Sensor!");
                return;
            }
            bool ok = MyGlobal.GoSDK.Start(ref error);
            if (ok != true)
            {
                MessageBox.Show(error);
            }
            else
            {
                MessageBox.Show("打开成功！");
            }
        }


        ///// <summary>
        /////保存的数据
        ///// </summary>
        //public static double[,] RCAll;

        //public static double[][] RArray;
        //public static double[][] CArray;

        //public static double[][] Row;
        //public static double[][] Phi;

        //public static double StartFov = 0;
        //public static double Resolution;
        //public static double yResolution;

        private void simpleButton2_Click(object sender, EventArgs e)//关闭激光
        {
            string error = "";
            bool ok = MyGlobal.GoSDK.Stop(ref error);
            MyGlobal.GoSDK.EnableProfle = false;
            if (ok != true)
            {
                MessageBox.Show(error);
                return;
            }

            try
            {

                List<SagensSdk.Profile> profile = MyGlobal.GoSDK.ProfileList;
                if (profile != null)
                {

                    //MyGlobal.globalConfig.dataContext = MyGlobal.GoSDK.context;

                    long SurfaceWidth, SurfaceHeight;
                    SurfaceWidth = profile[0].points.Length;
                    SurfaceHeight = profile.Count;
                    float[] SurfacePointZ = new float[SurfaceWidth * SurfaceHeight];

                    HObject HeightImage = new HObject(); HObject IntensityImage = new HObject();

                    //fpTool.GenIntesityProfile(profile, out IntensityImage);
                    MyGlobal.GoSDK.ProfileListToArr(profile, SurfacePointZ);
                    MyGlobal.GoSDK.GenHalconImage(SurfacePointZ, SurfaceWidth, SurfaceHeight, out HeightImage);

                    hwindow_final2.HobjectToHimage(IntensityImage);
                    //HOperatorSet.WriteImage(HeightImage, "tiff", 0, MyGlobal.ModelPath + "\\" + SideName + "H.tiff");
                    //HOperatorSet.WriteImage(IntensityImage, "tiff", 0, MyGlobal.ModelPath + "\\" + SideName + "I.tiff");
                    MyGlobal.hWindow_Final[0].HobjectToHimage(IntensityImage);
                }

                if (MyGlobal.GoSDK.SurfaceDataZ == null)
                {
                    MessageBox.Show("未收到数据");
                    return;

                }

  
                simpleButton3.Enabled = true;
                simpleButton4.Enabled = true;
                MessageBox.Show("OK");
            }
            catch (Exception ex)
            {

                MessageBox.Show(ex.Message);
            }
        }
        //List<HTuple> RowCoord = new List<HTuple>();
        //List<HTuple> ColCoord = new List<HTuple>();
        private void simpleButton1_Click(object sender, EventArgs e)
        {
            try
            {
                FolderBrowserDialog opf = new FolderBrowserDialog();


                opf.SelectedPath = "\\";
                if (opf.ShowDialog() == DialogResult.OK)
                {
                    //string file = opf.FileName;
                    //if (!file.Contains(".tiff"))
                    //{
                    //    return;
                    //}
                    SelectedPath = opf.SelectedPath;
                    ChangeSide();

                    simpleButton3.Enabled = true;
                    simpleButton4.Enabled = true;
                }

            }
            catch (Exception)
            {

                throw;
            }
        }
        string SelectedPath = "";
        bool NotUseFix = false;
        private void ChangeSide()
        {
            CurrentRowIndex = 0;
            fpTool.Init(FindType, isRight, ToolType);
            textBox_Current.Text = "0";
            textBox_Total.Text = "0";

            CurrentIndex = 0;

            int Id = Convert.ToInt32(SideName.Substring(4, 1)) - 1;

            string Path1 = "";


            if (MyGlobal.ImageMulti.Count >= Id + 1 && SelectedPath == "")
            {

                IntensityImage = MyGlobal.ImageMulti[Id][0];
                if (MyGlobal.globalConfig.enableAlign)
                {
                    if (MyGlobal.ImageMulti[Id].Length < 3)
                    {
                        MessageBox.Show("请重新加载数据");
                        return;
                    }
                    HeightImage = MyGlobal.ImageMulti[Id][2];
                    OriginImage = MyGlobal.ImageMulti[Id][1];
                }
                else
                {
                    HeightImage = MyGlobal.ImageMulti[Id][1];
                    OriginImage = null;
                }
               
            }
            else
            {
                RoiIsMoving = true;
                LoadToUI(Id);
                RoiIsMoving = false;
                return;
                //读取指定路径下或选择路径下图片
                if (File.Exists(Path1 + "\\" + SideName + "H.tiff"))
                {
                    //HeightImage.Dispose();
                    HOperatorSet.ReadImage(out HeightImage, Path1 + "\\" + SideName + "H.tiff");
                }
                if (File.Exists(Path1 + "\\" + SideName + "I.tiff"))
                {
                    //IntensityImage.Dispose();
                    HOperatorSet.ReadImage(out IntensityImage, Path1 + "\\" + SideName + "I.tiff");
                }
            }
            if (MyGlobal.GlueImageMulti.Count >= Id + 1)
            {
                GlueHeightImage = MyGlobal.GlueImageMulti[Id][1];
            }


            if (!HeightImage.IsInitialized())
            {
                return;
            }

            //PseudoColor.GrayToPseudoColor(HeightImage, out RGBImage, true, -20, 10);
            //PseudoColor.HeightAreaToPseudoColor(HeightImage, out RGBImage, -20, 10, fpTool.fParam[Id].MinZ, fpTool.fParam[Id].MaxZ);
            //hwindow_final2.HobjectToHimage(RGBImage);

            hwindow_final2.HobjectToHimage(IntensityImage);



            if (!NotUseFix && !FindType.Contains("Fix"))
            {
                string ok = "";
                if (ToolType == MyGlobal.ToolType_GlueGuide)
                {
                    if (isRight)
                    {
                        ok = MyGlobal.Right_findPointTool_Fix.FindIntersectPoint(Id + 1, HeightImage, out intersection, hwindow_final2, true);
                    }
                    else
                    {
                        ok = MyGlobal.Left_findPointTool_Fix.FindIntersectPoint(Id + 1, HeightImage, out intersection, hwindow_final2, true);
                    }
                }
                else
                {
                    //标定
                    if (isRight)
                    {
                        ok = MyGlobal.Right_Calib_Fix.FindIntersectPoint(Id + 1, HeightImage, out intersection, hwindow_final2, true);
                    }
                    else
                    {
                        ok = MyGlobal.Left_Calib_Fix.FindIntersectPoint(Id + 1, HeightImage, out intersection, hwindow_final2, true);
                    }
                }

                if (ok != "OK")
                {
                    MessageBox.Show(ok);
                }
                HTuple homMaxFix = new HTuple();
                double orignalDeg = fpTool.intersectCoordList[Id].Angle;
                double currentDeg = intersection.Angle;
                HOperatorSet.VectorAngleToRigid(fpTool.intersectCoordList[Id].Row, fpTool.intersectCoordList[Id].Col,
                    orignalDeg, intersection.Row, intersection.Col, currentDeg, out homMaxFix);
                //HOperatorSet.VectorAngleToRigid(0, 0, 0, 0, 0, 0, out homMaxFix);

                //转换Roi
                if (fpTool.roiList2[Id].Count > 0 && homMaxFix.Length > 0)
                {
                    for (int i = 0; i < fpTool.roiList2[Id].Count; i++)
                    {
                        List<ROI> temproi = new List<ROI>();
                        HTuple tempR = new HTuple(); HTuple tempC = new HTuple();
                        HTuple orignal = fpTool.roiList2[Id][i].getModelData();
                        HOperatorSet.AffineTransPoint2d(homMaxFix, orignal[0], orignal[1], out tempR, out tempC);
                        roiController2.viewController.ShowAllRoiModel = -1;
                        //角度
                        //角度                      
                        HTuple sx, sy, theta, deltaAngle, tx, ty;
                        HOperatorSet.HomMat2dToAffinePar(homMaxFix, out sx, out sy, out deltaAngle, out theta, out tx, out ty);
                        double tempPhi = orignal[2] + deltaAngle;
                        hwindow_final2.viewWindow.genRect2(tempR, tempC, tempPhi, orignal[3], orignal[4], ref temproi);
                        fpTool.roiList2[Id][i] = temproi[0];
                    }
                    roiController2.viewController.ShowAllRoiModel = 0;
                    roiController2.viewController.repaint(0);
                }
            }
            else
            {
                if (fpTool.roiList2[Id].Count > 0)
                {
                    fpTool.roiList2[Id].Clear();
                    fpTool.Init(FindType, isRight, ToolType);
                    hwindow_final2.viewWindow.displayROI(ref fpTool.roiList2[Id]);
                    roiController2.viewController.ShowAllRoiModel = 0;
                    roiController2.viewController.repaint(0);
                }
            }
            
            RoiIsMoving = true;
            LoadToUI(Id);
            RoiIsMoving = false;
            //textBox_SingleOffset.Text = fpTool.fParam[Id].SigleZoffset.ToString();
            //textBox_Total.Text = MyGlobal.globalConfig.TotalZoffset.ToString();
            FindPointTool.RArray = null;
            FindPointTool.CArray = null;
            FindPointTool.Row = null;
            //if (fpTool.roiList[Id].Count > 0)
            //{
            //    hwindow_final1.viewWindow.displayROI(ref fpTool.roiList[Id]);
            //}


            //HSystem.SetSystem("flush_graphic", "true");

        }

        void UpdateRoiParam()
        {
            for (int i = 0; i < fpTool.roiList2.Length; i++)
            {
                for (int j = 0; j < fpTool.roiList2[i].Count; j++)
                {
                    HTuple roiData = fpTool.roiList2[i][j].getModelData();
                    fpTool.fParam[i].roiP[j].CenterRow = roiData[0];
                    fpTool.fParam[i].roiP[j].CenterCol = roiData[1];
                    fpTool.fParam[i].roiP[j].phi = roiData[2];
                    fpTool.fParam[i].roiP[j].Len1 = roiData[3];
                    fpTool.fParam[i].roiP[j].Len2 = roiData[4];
                }
            }
        }
        public void GetCurrentFix()
        {
            try
            {
                int Id = Convert.ToInt32(SideName.Substring(4, 1)) - 1;
                IntersetionCoord intersect = new IntersetionCoord();
                //string ok = MyGlobal.flset2.FindIntersectPoint(Id + 1, HeightImage, out intersect);
                HTuple homMaxFix = new HTuple();
                HOperatorSet.VectorAngleToRigid(fpTool.intersectCoordList[Id].Row, fpTool.intersectCoordList[Id].Col,
                    0, intersect.Row, intersect.Col, 0, out homMaxFix);
                fpTool.intersectCoordList[Id] = intersect;
            }
            catch (Exception)
            {

                throw;
            }
        }

        int trackBarValue1 = 0;
        int trackBarValue2 = 0;
        private void trackBar1_Scroll(object sender, EventArgs e)
        {
            try
            {
                TrackBar tbar = (TrackBar)sender;
                //if (tbar.Name == "trackBar1")
                //{
                //    label_S.Text = trackBar1.Value.ToString();
                //    trackBarValue1 = trackBar1.Value;
                //}
                //else
                //{
                //    label_E.Text = trackBar2.Value.ToString();
                //    trackBarValue2 = trackBar2.Value;
                //}
                //button11_Click(sender, e);

                int Id = Convert.ToInt32(SideName.Substring(4, 1));
                HTuple row, col; HTuple anchor, anchorc;
                //fpTool.FindMaxPt(Id, CurrentIndex - 1, out row, out col, out anchor, out anchorc, hwindow_final1, ShowSection);
                int roiID = -1;
                for (int i = 0; i < fpTool.fParam[Id - 1].roiP.Count; i++)
                {

                    for (int j = 0; j < fpTool.fParam[Id - 1].roiP[i].NumOfSection; j++)
                    {
                        roiID++;
                        if (roiID == CurrentIndex - 1)
                        {
                            break;
                        }
                    }
                    if (roiID == CurrentIndex - 1)
                    {
                        roiID = i;
                        break;
                    }

                }
                if (fpTool.fParam[Id - 1].roiP[roiID].SelectedType == 0)
                {
                    if (fpTool.fParam[Id - 1].roiP[roiID].TopDownDist != 0 && fpTool.fParam[Id - 1].roiP[roiID].xDist != 0)
                    {
                        fpTool.FindMaxPtFallDown(Id, CurrentIndex - 1, out row, out col, out anchor, out anchorc, hwindow_final1, ShowSection, false, hwindow_final2);
                    }
                    else
                    {
                        fpTool.FindMaxPt(Id, CurrentIndex - 1, out row, out col, out anchor, out anchorc, hwindow_final2, ShowSection, false, hwindow_final1);
                    }


                }
                else
                {
                    fpTool.FindMaxPtFallDown(Id, CurrentIndex - 1, out row, out col, out anchor, out anchorc, hwindow_final1, ShowSection, false, hwindow_final2);
                }
                //if (trackBarValue1<trackBarValue2)
                //{
                //    HTuple row2, col2;
                //    ShowProfile(out row2,out col2);

                //    HTuple newRow = row2; HTuple newCol = col2;
                //    int start = 0; int end = 0;
                //    if (trackBarValue2 >= row2.Length)
                //    {
                //        trackBarValue2 = row2.Length - 1;
                //    }

                //    start = trackBarValue1;
                //    end = trackBarValue2;

                //    HTuple rowLast = newRow.TupleSelectRange(start, end);
                //    HTuple colLast = newCol.TupleSelectRange(start, end);
                //    HObject ContourSelect = new HObject();
                //    HOperatorSet.GenRegionPoints(out ContourSelect, rowLast, colLast);
                //    //HOperatorSet.GenContourPolygonXld(out ContourSelect, rowLast, colLast);
                //    hwindow_final1.viewWindow.displayHobject(ContourSelect, "blue");
                //}
            }
            catch (Exception)
            {

                throw;
            }
        }


        private void button11_Click(object sender, EventArgs e)
        {
            if (trackBarValue1 < trackBarValue2 && FindPointTool.RArray.GetLength(0) > 0)
            {


                if (trackBarValue1 < trackBarValue2)
                {
                    HTuple row2 = new HTuple(), col2 = new HTuple();
                    //ShowProfile(CurrentIndex - 1, out row2, out col2);

                    HTuple newRow = row2; HTuple newCol = col2;
                    int start = 0; int end = 0;
                    if (trackBarValue2 >= row2.Length)
                    {
                        trackBarValue2 = row2.Length - 1;
                    }

                    start = trackBarValue1;
                    end = trackBarValue2;

                    HTuple rowLast = newRow.TupleSelectRange(start, end);
                    HTuple colLast = newCol.TupleSelectRange(start, end);
                    HObject ContourSelect = new HObject();
                    HOperatorSet.GenRegionPoints(out ContourSelect, rowLast, colLast);
                    //HOperatorSet.GenContourPolygonXld(out ContourSelect, rowLast, colLast);
                    if (ShowSection)
                    {
                        hwindow_final1.viewWindow.displayHobject(ContourSelect, "blue");
                    }

                }
            }

        }

        private void cb_LorR_CheckedChanged(object sender, EventArgs e)
        {
            int Id = Convert.ToInt32(SideName.Substring(4, 1)) - 1;
            //fpTool.fParam[Id].BeLeft = cb_LorR.Checked;
        }

        private void textBox_Start_TextChanged(object sender, EventArgs e)
        {
            if (isLoading)
            {
                return;
            }
            TextBox tb = (TextBox)sender;
            string Num = tb.Text.ToString();
            bool ok = Regex.IsMatch(Num, @"(?i)^(\-[0-9]{1,}[.][0-9]*)+$") || Regex.IsMatch(Num, @"(?i)^(\-[0-9]{1,}[0-9]*)+$") || Regex.IsMatch(Num, @"(?i)^([0-9]{1,}[0-9]*)+$");
            if (ok)
            {
                int Id = Convert.ToInt32(SideName.Substring(4, 1)) - 1;
                switch (tb.Name)
                {
                    case "textBox_Start":
                        //fpTool.fParam[Id].StartPt = int.Parse(Num);
                        break;
                    case "textBox_End":
                        //fpTool.fParam[Id].EndPt = int.Parse(Num);
                        break;
                    case "tb_Updown":
                        //fpTool.fParam[Id].UpDownDist = double.Parse(Num);
                        break;

                }

            }

        }

        private void button1_Click(object sender, EventArgs e)
        {
            HTuple Row = new HTuple(), Col = new HTuple();
            int Id = Convert.ToInt32(SideName.Substring(4, 1));
            //string ok =  Fit(Id ,out Row, out Col,hwindow_final1);

            for (int i = 0; i < FindPointTool.RArray.GetLength(0); i++)
            {
                HTuple row, col; HTuple anchor, anchorc;
                fpTool.FindMaxPt(Id, i, out row, out col, out anchor, out anchorc, hwindow_final1);

                Row = Row.TupleConcat(row);
                Col = Col.TupleConcat(col);
            }
            //if (ok!="OK")
            //{
            //    MessageBox.Show(ok);
            //}
            HObject line = new HObject();
            HOperatorSet.GenContourPolygonXld(out line, Row, Col);

            HTuple Rowbg, Colbg, RowEd, ColEd, Nr, Nc, Dist;
            HOperatorSet.FitLineContourXld(line, "tukey", -1, 0, 5, 2, out Rowbg, out Colbg, out RowEd, out ColEd, out Nr, out Nc, out Dist);
            HObject Contourline = new HObject();
            HOperatorSet.GenContourPolygonXld(out Contourline, Rowbg.TupleConcat(RowEd), Colbg.TupleConcat(ColEd));
            //HOperatorSet.SmoothContoursXld(line, out Contourline, 25);           
            hwindow_final2.viewWindow.displayHobject(line);

            hwindow_final2.viewWindow.displayHobject(Contourline, "green");
            MessageBox.Show("拟合成功！");
        }

        private void simpleButton4_Click(object sender, EventArgs e)//上一帧
        {
            if (CurrentIndex == 0)
            {
                return;
            }
            if (CurrentIndex == 1)
            {
                CurrentIndex = 1;
                textBox_Current.Text = CurrentIndex.ToString();
                return;
            }
            if (CurrentIndex > 1)
            {
                CurrentIndex--;
                trackBarControl1.Value = CurrentIndex;
                textBox_Current.Text = CurrentIndex.ToString();

                //button11_Click(sender, e);
                int Id = Convert.ToInt32(SideName.Substring(4, 1));
                HTuple row, col; HTuple anchor, anchorc;
                //fpTool.FindMaxPt(Id, CurrentIndex - 1, out row, out col, out anchor, out anchorc, hwindow_final1, ShowSection);
                int roiID = -1;
                for (int i = 0; i < fpTool.fParam[Id - 1].roiP.Count; i++)
                {

                    for (int j = 0; j < fpTool.fParam[Id - 1].roiP[i].NumOfSection; j++)
                    {
                        roiID++;
                        if (roiID == CurrentIndex - 1)
                        {
                            break;
                        }
                    }
                    if (roiID == CurrentIndex - 1)
                    {
                        roiID = i;
                        break;
                    }

                }
                int CurrentRoiId = 0;
                int n = 0;
                for (int i = 0; i < roiID; i++)
                {
                    n = n + fpTool.fParam[Id - 1].roiP[i].NumOfSection;
                }
                CurrentRoiId = CurrentIndex - n;
                if (FindType == MyGlobal.FindPointType_Detect)
                {
                    if (pResult.Row != null)
                    {
                        string ok2 = fpTool.FindPoint_Detect(Id - 1, HeightImage, out pResult, null, hwindow_final2, hwindow_final1, ShowSection, roiID + 1, CurrentRoiId);
                        if (ok2 != "OK")
                        {
                            MessageBox.Show(ok2);
                        }
                        if (this.Text == "Detect" && GlueHeightImage != null && GlueHeightImage.IsInitialized())
                        {
                            ProfileResult GlueResult = new ProfileResult();
                            ok2 = fpTool.FindPoint_Detect(Id - 1, GlueHeightImage, out GlueResult, null, null, null, ShowSection, roiID + 1, CurrentRoiId);
                            HObject reg;
                            HOperatorSet.GenRegionPoints(out reg, new HTuple(GlueResult.ZCoord[CurrentIndex - 1]), new HTuple(GlueResult.ProfileCol[CurrentIndex - 1]));
                            hwindow_final1.viewWindow.displayHobject(reg, "green");
                            reg.Dispose();
                            DetectResult detectRst;
                            fpTool.SubProfile(pResult, GlueResult, Id - 1,roiID,  CurrentRoiId - 1,out detectRst, hwindow_final3);
                            ShowDetectResult(detectRst);
                        }
                    }

                }
                else
                {
                    if (fpTool.fParam[Id - 1].roiP[roiID].SelectedType == 0)
                    {
                        if (fpTool.fParam[Id - 1].roiP[roiID].TopDownDist != 0 && fpTool.fParam[Id - 1].roiP[roiID].xDist != 0)
                        {
                            fpTool.FindMaxPtFallDown(Id, CurrentIndex - 1, out row, out col, out anchor, out anchorc, hwindow_final1, ShowSection, false, hwindow_final2);
                        }
                        else
                        {
                            fpTool.FindMaxPt(Id, CurrentIndex - 1, out row, out col, out anchor, out anchorc, hwindow_final2, ShowSection, false, hwindow_final1);
                        }

                    }
                    else
                    {
                        fpTool.FindMaxPtFallDown(Id, CurrentIndex - 1, out row, out col, out anchor, out anchorc, hwindow_final1, ShowSection, false, hwindow_final2);
                    }
                }
            }
        }

        private void simpleButton3_Click(object sender, EventArgs e)//下一帧
        {
            int Id = Convert.ToInt32(SideName.Substring(4, 1));
            int Total = Convert.ToInt32(textBox_Total.Text.ToString());
            if (CurrentIndex >= Total)
            {
                CurrentIndex = Total;
                textBox_Current.Text = CurrentIndex.ToString();
                trackBarControl1.Value = CurrentIndex;
                return;
            }
            CurrentIndex++;
            trackBarControl1.Value = CurrentIndex;
            textBox_Current.Text = CurrentIndex.ToString();
            int roiID = -1;
            for (int i = 0; i < fpTool.fParam[Id - 1].roiP.Count; i++)
            {
                for (int j = 0; j < fpTool.fParam[Id - 1].roiP[i].NumOfSection; j++)
                {
                    roiID++;
                    if (roiID == CurrentIndex - 1)
                    {
                        break;
                    }
                }
                if (roiID == CurrentIndex - 1)
                {
                    roiID = i;
                    break;
                }

            }

            int CurrentRoiId = 0;
            int n = 0;
            for (int i = 0; i < roiID; i++)
            {
                n = n + fpTool.fParam[Id - 1].roiP[i].NumOfSection;
            }
            CurrentRoiId = CurrentIndex - n;

            if (FindType == MyGlobal.FindPointType_Detect)
            {
                if (pResult.Row != null)
                {
                    string ok2 = fpTool.FindPoint_Detect(Id - 1, HeightImage, out pResult, null, hwindow_final2, hwindow_final1, ShowSection, roiID + 1, CurrentRoiId);
                    if (ok2 != "OK")
                    {
                        MessageBox.Show(ok2);
                    }
                    if (this.Text == "Detect" && GlueHeightImage != null && GlueHeightImage.IsInitialized())
                    {
                        ProfileResult GlueResult = new ProfileResult();
                        ok2 = fpTool.FindPoint_Detect(Id - 1, GlueHeightImage, out GlueResult, null, null, null, ShowSection, roiID + 1, CurrentRoiId);
                        HObject reg;
                        HOperatorSet.GenRegionPoints(out reg, new HTuple(GlueResult.ZCoord[CurrentIndex - 1]),new HTuple(GlueResult.ProfileCol[CurrentIndex - 1]));
                        hwindow_final1.viewWindow.displayHobject(reg, "green");
                        reg.Dispose();
                        DetectResult detectRst;
                        fpTool.SubProfile(pResult, GlueResult, Id - 1, roiID,CurrentRoiId - 1,out detectRst, hwindow_final3);
                        ShowDetectResult(detectRst);

                    }
                }

            }
            else
            {
                HTuple row, col; HTuple anchor, anchorc;
               
                if (fpTool.fParam[Id - 1].roiP[roiID].SelectedType == 0)
                {
                    if (fpTool.fParam[Id - 1].roiP[roiID].TopDownDist != 0 && fpTool.fParam[Id - 1].roiP[roiID].xDist != 0)
                    {
                        fpTool.FindMaxPtFallDown(Id, CurrentIndex - 1, out row, out col, out anchor, out anchorc, hwindow_final1, ShowSection, false, hwindow_final2);
                    }
                    else
                    {
                        fpTool.FindMaxPt(Id, CurrentIndex - 1, out row, out col, out anchor, out anchorc, hwindow_final2, ShowSection, false, hwindow_final1);
                    }

                }
                else
                {
                    fpTool.FindMaxPtFallDown(Id, CurrentIndex - 1, out row, out col, out anchor, out anchorc, hwindow_final1, ShowSection, false, hwindow_final2);
                }
            }

        }
        bool ShowSection = false;
        private void checkBox1_CheckedChanged(object sender, EventArgs e)//显示
        {
            ShowSection = checkBox1.Checked;
        }
        string SideName = "Side1";

        public void FitLineParamSave()
        {
            int Id = Convert.ToInt32(SideName.Substring(4, 1)) - 1;
            ParamPath.ToolType = ToolType;
            ParamPath.ParaName = FindType + "_" + SideName;
            ParamPath.IsRight = isRight;
            string Name = ToolType;
         
            StaticOperate.WriteXML(fpTool.fParam[Id], ParamPath.ParamDir + SideName + ".xml");
            hwindow_final1.viewWindow.saveROI(fpTool.roiList[Id], ParamPath.ParamDir + SideName + "_Section.roi");
            hwindow_final2.viewWindow.saveROI(fpTool.roiList2[Id], ParamPath.ParamDir + SideName + "_Region.roi");
            if (this.Text == "Detect")
            {
                hwindow_final2.viewWindow.saveROI(fpTool.DetectRoiList[Id], ParamPath.ParamDir + SideName + "_detect.roi");
                StaticOperate.WriteXML(fpTool.detectParam[Id], ParamPath.ParamDir + SideName + "_DetectParam.xml");
            }
            isSave = true;

            if (FindType == "Fix")
            {
                if (MessageBox.Show("是否重新写入模板位置？", "提示", MessageBoxButtons.YesNo) == DialogResult.Yes)
                {
                    fpTool.intersectCoordList[Id] = intersection;
                    if (intersection.Row==0 && intersection.Col==0)
                    {
                        MessageBox.Show("当前模板未运行！请运行成功后再写入！");
                    }
                    else
                    {
                        StaticOperate.WriteXML(fpTool.intersectCoordList[Id], ParamPath.Path_Param);

                    }
                }
            }
            else if (FindType == "FitLineSet")
            {
                if (intersection.Row == 0 && intersection.Col == 0 && NotUseFix)
                {
                    MessageBox.Show("保存成功！");
                }
                else
                {
                    ParamPath.ParaName = "Fix" + "_" + SideName;
                    fpTool.intersectCoordList[Id] = intersection;
                    StaticOperate.WriteXML(fpTool.intersectCoordList[Id], ParamPath.Path_Param);
                }
            }
            else if (FindType == "Detect")
            {
                if (intersection.Row == 0 && intersection.Col == 0 && NotUseFix)
                {
                    MessageBox.Show("保存成功！");
                }
                else
                {
                    ParamPath.ParaName = "Fix" + "_" + SideName;
                    fpTool.intersectCoordList[Id] = intersection;
                    StaticOperate.WriteXML(fpTool.intersectCoordList[Id], ParamPath.Path_Param);
                }
            }
            

            //MyGlobal.flset2.Init();
            if (ToolType == MyGlobal.ToolType_GlueGuide)
            {
                if (isRight)
                {
                    MyGlobal.Right_findPointTool_Find.Init("FitLineSet", isRight, ToolType);
                    MyGlobal.Right_findPointTool_Fix.Init("Fix", isRight, ToolType);
                    MyGlobal.Right_findPointTool_Find_Detect.Init("Detect", isRight, ToolType);
                }
                else
                {
                    MyGlobal.Left_findPointTool_Find.Init("FitLineSet", isRight, ToolType);
                    MyGlobal.Left_findPointTool_Fix.Init("Fix", isRight, ToolType);
                    MyGlobal.Left_findPointTool_Find_Detect.Init("Detect", isRight, ToolType);
                }
            }
            else
            {
                if (isRight)
                {
                    //MyGlobal.Right_findPointTool_Find.Init("FitLineSet", isRight, ToolType);
                    MyGlobal.Right_Calib_Fix.Init("Fix", isRight, ToolType);
                }
                else
                {
                    //MyGlobal.Left_findPointTool_Find.Init("FitLineSet", isRight, ToolType);
                    MyGlobal.Left_Calib_Fix.Init("Fix", isRight, ToolType);
                }
            }



            MessageBox.Show("保存成功！");
        }

        private void button2_Click(object sender, EventArgs e)//保存参数
        {
            RoiParam.isInvoke = false;
            if (isRight)
            {
                if (MessageBox.Show("是否保存右工位参数", "提示", MessageBoxButtons.YesNo) == DialogResult.Yes)
                {
                    FitLineParamSave();
                }
            }
            else
            {
                if (MessageBox.Show("是否保存左工位参数", "提示", MessageBoxButtons.YesNo) == DialogResult.Yes)
                {
                    FitLineParamSave();
                }

            }
            RoiParam.isInvoke = true;
            //simpleButton7_Click(sender, e);

        }

        string CurrentSide = "";
        bool isSave = true;
        bool NoChange = false;
        bool isCloing = false;
        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)//选择边
        {

            if (isCloing || NoChange || comboBox1.SelectedItem.ToString() == CurrentSide)
            {
                isCloing = false;
                NoChange = false;
                return;
            }
            if (comboBox1.SelectedItem != null)
            {
                if (isSave)
                {
                    //isSave = false;

                }
                else
                {
                    DialogResult result = MessageBox.Show("当前参数未保存，是否切换?", "提示：", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

                    if (result != DialogResult.Yes)
                    {

                        if (comboBox1.SelectedItem.ToString() != CurrentSide)
                        {
                            NoChange = true;
                            comboBox1.SelectedItem = CurrentSide;
                            NoChange = false;
                        }

                        return;
                    }
                }


                SideName = comboBox1.SelectedItem.ToString();
                ParamPath.ParaName = FindType + "_" + SideName;
                ParamPath.IsRight = isRight;
                hwindow_final1.viewWindow.notDisplayRoi();
                hwindow_final2.viewWindow.notDisplayRoi();
                hwindow_final1.ClearWindow();
                hwindow_final2.ClearWindow();
                RoiParam.isInvoke = false;
                ChangeSide();
                RoiParam.isInvoke = true;
                CurrentSide = comboBox1.SelectedItem.ToString();
                if (!HeightImage.IsInitialized())
                {
                    return;
                }


               

                //int Id = Convert.ToInt32(SideName.Substring(4, 1)) - 1;
                //if (File.Exists(MyGlobal.ConfigPath + SideName + ".xml"))
                //{
                //   fpTool.fParam[Id] = (FitProfileParam)StaticOperate.ReadXML(MyGlobal.ConfigPath + SideName + ".xml", typeof(FitProfileParam));
                //   LoadToUI(Id);
                //}
                //if (File.Exists(MyGlobal.ConfigPath + SideName[Id] + "_Section.roi"))
                //{
                //    hwindow_final1.viewWindow.loadROI(MyGlobal.ConfigPath + SideName[Id] + "_Section.roi", out fpTool.roiList[Id]);
                //}            
                //if (File.Exists(MyGlobal.ConfigPath + SideName[Id] + "_Region.roi"))
                //{
                //    hwindow_final2.viewWindow.loadROI(MyGlobal.ConfigPath + SideName[Id] + "_Region.roi", out fpTool.fpTool.roiList2[Id]);
                //}
            }
        }
        private void button3_Click(object sender, EventArgs e)
        {
            //if (roiCount==1 || RArray.GetLength(0)==0)
            //{
            //    hwindow_final1.viewWindow.displayROI(ref fpTool.roiList);
            //    return;
            //}
            //roiController.setROIShape(new ROIRectangle1());          
            //roiCount++;
        }
        bool isGenSection = false;
        private void button4_Click(object sender, EventArgs e)//截图区域
        {
            int Id = Convert.ToInt32(SideName.Substring(4, 1)) - 1;
            hwindow_final2.viewWindow.notDisplayRoi();
            if (fpTool.roiList2[Id].Count > 0)
            {
                if (SelectAll)
                {
                    roiController2.viewController.ShowAllRoiModel = -1;
                }
                hwindow_final2.viewWindow.displayROI(ref fpTool.roiList2[Id]);
            }


            if (hwindow_final2.Image == null || !hwindow_final2.Image.IsInitialized())
            {
                return;
            }
            roiController2.setROIShape(new ROIRectangle2());
            isGenSection = true;
            //fpTool.fpTool.roiList2[Id].Add(new ROIRectangle2());
        }

        private void 删除ToolStripMenuItem_Click(object sender, EventArgs e)//右键删除
        {
            int Id = Convert.ToInt32(SideName.Substring(4, 1)) - 1;

            if (dataGridView1.CurrentCell == null)
            {
                return;
            }
            int RowId = dataGridView1.CurrentCell.RowIndex;
            RowId = CurrentRowIndex;
            string Name = dataGridView1.Rows[RowId].Cells[1].Value.ToString();

            if (fpTool.roiList2[Id].Count > 0 && roiController2.ROIList.Count == fpTool.roiList2[Id].Count)
            {
                if (RowId >= 0)
                {
                    fpTool.roiList2[Id].RemoveAt(RowId);
                    roiController2.setActiveROIIdx(RowId);
                    roiController2.removeActive();

                    fpTool.fParam[Id].roiP.RemoveAt(RowId);
                    fpTool.roiList[Id].RemoveAt(RowId);
                    if (this.Text == "Detect")
                    {
                        fpTool.DetectRoiList[Id].RemoveAt(RowId);
                    }
                    fpTool.fParam[Id].DicPointName.RemoveAt(RowId);
                    dataGridView1.Rows.RemoveAt(RowId);
                    //排序
                    for (int i = 0; i < dataGridView1.Rows.Count; i++)
                    {
                        dataGridView1.Rows[i].Cells[0].Value = i + 1;
                    }

                    //int[] values = DicPointName[Id].Values.ToArray();
                    //string[] keys = DicPointName[Id].Keys.ToArray();
                    ////listBox1.Items.CopyTo(keys, 0);
                    //keys = richTextBox1.Lines;
                    //for (int i = Index; i < DicPointName[Id].Count; i++)
                    //{

                    //        int value1 = DicPointName[Id][keys[i]];
                    //    DicPointName[Id][keys[i]] = value1 - 1;//前移

                    //}      
                }
                //刷新界面roi


                hwindow_final2.viewWindow.notDisplayRoi();
                if (fpTool.roiList2[Id].Count > 0)
                {
                    hwindow_final2.viewWindow.displayROI(ref fpTool.roiList2[Id]);
                }
                if (fpTool.roiList2[Id].Count > 0)
                {
                    hwindow_final2.viewWindow.selectROI(0);
                }
                if (RowId == CopyId)
                {
                    CopyId = -1;
                }
            }
        }

        private void 删除所有ToolStripMenuItem_Click(object sender, EventArgs e)//右键删除所有
        {
            if (dataGridView1.Rows.Count == 0)
            {
                return;
            }
            if (!(MessageBox.Show("是否删除所有区域", "提示", MessageBoxButtons.YesNo) == DialogResult.Yes))
            {
                return;
            }
            int Id = Convert.ToInt32(SideName.Substring(4, 1)) - 1;
            if (fpTool.roiList2[Id].Count > 0)
            {
                fpTool.roiList2[Id].Clear();
                hwindow_final2.viewWindow.notDisplayRoi();
                dataGridView1.Rows.Clear();


                fpTool.roiList[Id].Clear();
                hwindow_final1.viewWindow.notDisplayRoi();
                PreSelect = "";
                fpTool.fParam[Id].DicPointName.Clear();
                if (Id >= 0)
                {
                    fpTool.fParam[Id].roiP.Clear();
                }
            }
            CurrentRowIndex = -1;
            CopyId = -1;
        }
        List<ROI> temp = new List<ROI>();
        List<ROI> temp_Detect = new List<ROI>();
        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            int SideId = Convert.ToInt32(SideName.Substring(4, 1)) - 1;
            TextBox tb = (TextBox)sender;
            string index = tb.Text.ToString();
            bool ok1 = Regex.IsMatch(index, @"^[-]?\d+[.]?\d*$");//是否为数字
            bool ok = Regex.IsMatch(index, @"^([-]?)\d*$");//是否为整数
            if (!(ok && ok1) || fpTool.roiList2[SideId].Count == 0 || RoiIsMoving)
            {
                return;
            }
            try
            {


                double a = Convert.ToDouble(index);
                int num = (int)a;

                int roiID = dataGridView1.CurrentCell.RowIndex;
                roiID = CurrentRowIndex;
                int count = dataGridView1.SelectedCells.Count;
                List<int> rowInd = new List<int>();
                for (int i = 0; i < count; i++)
                {
                    int Ind = dataGridView1.SelectedCells[i].RowIndex;
                    if (!rowInd.Contains(Ind))
                    {
                        rowInd.Add(Ind);
                    }
                }
                for (int i = 0; i < rowInd.Count; i++)
                {
                    roiID = rowInd[i];
                    HTuple[] lineCoord = new HTuple[1];
                    switch (tb.Name)
                    {
                        case "textBox_Num":
                            fpTool.fParam[SideId].roiP[roiID].NumOfSection = num;

                            fpTool.DispSection((ROIRectangle2)fpTool.roiList2[SideId][roiID], SideId, roiID, out lineCoord, hwindow_final2);
                            break;
                        case "textBox_Len":
                            fpTool.fParam[SideId].roiP[roiID].Len1 = num;
                            HTuple temp = new HTuple();
                            List<ROI> temproi = new List<ROI>();
                            temp = fpTool.roiList2[SideId][roiID].getModelData();
                            hwindow_final2.viewWindow.genRect2(fpTool.fParam[SideId].roiP[roiID].CenterRow, fpTool.fParam[SideId].roiP[roiID].CenterCol, fpTool.fParam[SideId].roiP[roiID].phi, fpTool.fParam[SideId].roiP[roiID].Len1, fpTool.fParam[SideId].roiP[roiID].Len2, ref temproi);
                            //hwindow_final2.viewWindow.genRect2(temp[0].D, temp[1].D, temp[2].D, fpTool.fParam[SideId].roiP[roiID].Len1, fpTool.fParam[SideId].roiP[roiID].Len2, ref temproi);
                            fpTool.roiList2[SideId][roiID] = temproi[0];
                            //hwindow_final2.viewWindow.notDisplayRoi();
                            //hwindow_final2.viewWindow.displayROI(ref fpTool.roiList2[SideId]);
                            //hwindow_final2.viewWindow.selectROI(roiID);
                            //DispSection((ROIRectangle2)fpTool.roiList2[SideId][roiID], SideId, roiID, out lineCoord, hwindow_final2);
                            break;
                        case "textBox_Width":
                            fpTool.fParam[SideId].roiP[roiID].Len2 = num;
                            HTuple temp3 = new HTuple();
                            temp3 = fpTool.roiList2[SideId][roiID].getModelData();
                            List<ROI> temproi3 = new List<ROI>();
                            hwindow_final2.viewWindow.genRect2(fpTool.fParam[SideId].roiP[roiID].CenterRow, fpTool.fParam[SideId].roiP[roiID].CenterCol, fpTool.fParam[SideId].roiP[roiID].phi, fpTool.fParam[SideId].roiP[roiID].Len1, fpTool.fParam[SideId].roiP[roiID].Len2, ref temproi3);
                            fpTool.roiList2[SideId][roiID] = temproi3[0];
                            //hwindow_final2.viewWindow.notDisplayRoi();
                            //hwindow_final2.viewWindow.displayROI(ref fpTool.roiList2[SideId]);
                            //hwindow_final2.viewWindow.selectROI(roiID);
                            //DispSection((ROIRectangle2)fpTool.roiList2[SideId][roiID], SideId, roiID, out lineCoord, hwindow_final2);
                            break;
                        case "textBox_Row":
                            fpTool.fParam[SideId].roiP[roiID].CenterRow = num;
                            HTuple temp4 = new HTuple();
                            temp4 = fpTool.roiList2[SideId][roiID].getModelData();
                            List<ROI> temproi4 = new List<ROI>();
                            hwindow_final2.viewWindow.genRect2(fpTool.fParam[SideId].roiP[roiID].CenterRow, fpTool.fParam[SideId].roiP[roiID].CenterCol, fpTool.fParam[SideId].roiP[roiID].phi, fpTool.fParam[SideId].roiP[roiID].Len1, fpTool.fParam[SideId].roiP[roiID].Len2, ref temproi4);
                            fpTool.roiList2[SideId][roiID] = temproi4[0];
                            //hwindow_final2.viewWindow.notDisplayRoi();
                            //hwindow_final2.viewWindow.displayROI(ref fpTool.roiList2[SideId]);
                            //hwindow_final2.viewWindow.selectROI(roiID);
                            //DispSection((ROIRectangle2)fpTool.roiList2[SideId][roiID], SideId, roiID, out lineCoord, hwindow_final2);
                            break;
                        case "textBox_Col":
                            fpTool.fParam[SideId].roiP[roiID].CenterCol = num;
                            HTuple temp5 = new HTuple();
                            temp3 = fpTool.roiList2[SideId][roiID].getModelData();
                            List<ROI> temproi5 = new List<ROI>();
                            hwindow_final2.viewWindow.genRect2(fpTool.fParam[SideId].roiP[roiID].CenterRow, fpTool.fParam[SideId].roiP[roiID].CenterCol, fpTool.fParam[SideId].roiP[roiID].phi, fpTool.fParam[SideId].roiP[roiID].Len1, fpTool.fParam[SideId].roiP[roiID].Len2, ref temproi5);
                            fpTool.roiList2[SideId][roiID] = temproi5[0];
                            //hwindow_final2.viewWindow.notDisplayRoi();
                            //hwindow_final2.viewWindow.displayROI(ref fpTool.roiList2[SideId]);
                            //hwindow_final2.viewWindow.selectROI(roiID);
                            //DispSection((ROIRectangle2)fpTool.roiList2[SideId][roiID], SideId, roiID, out lineCoord, hwindow_final2);
                            break;
                        case "textBox_phi":
                            HTuple rad = new HTuple();
                            HOperatorSet.TupleRad(num, out rad);
                            fpTool.fParam[SideId].roiP[roiID].phi = rad;
                            HTuple temp6 = new HTuple();
                            temp3 = fpTool.roiList2[SideId][roiID].getModelData();
                            List<ROI> temproi6 = new List<ROI>();
                            hwindow_final2.viewWindow.genRect2(fpTool.fParam[SideId].roiP[roiID].CenterRow, fpTool.fParam[SideId].roiP[roiID].CenterCol, fpTool.fParam[SideId].roiP[roiID].phi, fpTool.fParam[SideId].roiP[roiID].Len1, fpTool.fParam[SideId].roiP[roiID].Len2, ref temproi6);
                            fpTool.roiList2[SideId][roiID] = temproi6[0];
                            //hwindow_final2.viewWindow.notDisplayRoi();
                            //hwindow_final2.viewWindow.displayROI(ref fpTool.roiList2[SideId]);
                            //hwindow_final2.viewWindow.selectROI(roiID);
                            //DispSection((ROIRectangle2)fpTool.roiList2[SideId][roiID], SideId, roiID, out lineCoord, hwindow_final2);
                            break;
                        case "textBox_Deg":
                            fpTool.fParam[SideId].roiP[roiID].AngleOfProfile = num;
                            break;
                    }
                }
                hwindow_final2.viewWindow.notDisplayRoi();
                hwindow_final2.viewWindow.displayROI(ref fpTool.roiList2[SideId]);
                hwindow_final2.viewWindow.selectROI(roiID);

            }
            catch (Exception)
            {

                throw;
            }

        }
        private int Ignore = 0;

        bool SelectAll = false;
        private void checkBox2_CheckedChanged(object sender, EventArgs e)//右键全选
        {
            SelectAll = checkBox2.Checked;
        }

        private void simpleButton7_Click(object sender, EventArgs e)//run按钮
        {
            try
            {
                if (/*!RGBImage.IsInitialized() || */!HeightImage.IsInitialized())
                {
                    return;
                }

                int Id = Convert.ToInt32(SideName.Substring(4, 1));
                double[][] Rcoord, Ccoord, Zcoord; string[][] Str;
                hwindow_final2.ClearWindow();
                //hwindow_final2.HobjectToHimage(RGBImage);

                hwindow_final2.HobjectToHimage(IntensityImage);
               
                switch (FindType)
                {
                    case MyGlobal.FindPointType_FitLineSet://引导
                        //ChangeSide已定位Roi 不用定位     
                        HTuple[] original = new HTuple[2];
                        string ok = fpTool.FindPoint(Id, isRight, HeightImage, HeightImage, out Rcoord, out Ccoord, out Zcoord, out Str, out original, null, hwindow_final2, true, null, OriginImage);
                        if (ok != "OK")
                        {
                            MessageBox.Show(ok);
                        }
                        break;
                    case MyGlobal.FindPointType_Detect: //检测
                        //ChangeSide已定位Roi 不用定位     
                        Stopwatch sp = new Stopwatch();
                        sp.Start();
                        HTuple[] original1 = new HTuple[2];
                        string ok3 = fpTool.FindPoint(Id, isRight, HeightImage, HeightImage, out Rcoord, out Ccoord, out Zcoord, out Str, out original1, null, hwindow_final2, true, null, OriginImage);
                        if (ok3 != "OK")
                        {
                            MessageBox.Show(ok3);
                        }
                        string ok2 = fpTool.FindPoint_Detect(Id - 1, HeightImage, out pResult, null, hwindow_final2);
                        if (ok2 != "OK")
                        {
                            MessageBox.Show(ok2);
                        }
                        sp.Stop();
                        MessageBox.Show(sp.ElapsedMilliseconds.ToString());
                        break;
                    case MyGlobal.FindPointType_Fix: //定位
                        string ok1 = "";
                        if (isRight)
                        {
                            ok1 = fpTool.FindIntersectPoint(Id, HeightImage, out intersection, hwindow_final2, true);
                        }
                        else
                        {
                            ok1 = fpTool.FindIntersectPoint(Id, HeightImage, out intersection, hwindow_final2, true);
                        }

                        if (ok1 != "OK")
                        {
                            MessageBox.Show(ok1);
                        }
                        break;
                }

                //if (ToolType == MyGlobal.ToolType_GlueGuide)
                //{
                   
                //}
                //else
                //{
                //    //标定
                //    if (FindType == "Fix")
                //    {
                //        string ok1 = "";
                //        ok1 = fpTool.FindIntersectPoint(Id, HeightImage, out intersection, hwindow_final2, true);
                //        if (ok1 != "OK")
                //        {
                //            MessageBox.Show(ok1);
                //        }
                //    }
                //    else
                //    {
                //        //ChangeSide已定位Roi 不用定位     
                //        HTuple[] original = new HTuple[2];
                //        string ok = fpTool.FindPoint(Id, HeightImage, HeightImage, out _lines, hwindow_final2, true, null, OriginImage);
                //        if (ok != "OK")
                //        {
                //            MessageBox.Show(ok);
                //        }
                //    }
                //}

                int Total = 0;
              
                if (FindType == MyGlobal.FindPointType_Detect)
                {
                    if (pResult.Row!=null)
                    {
                        Total = pResult.Row.GetLength(0);
                    }
                    
                }
                else if(FindPointTool.RArray != null)
                {
                    Total = FindPointTool.RArray.GetLength(0);
                }
                textBox_Total.Text = Total.ToString();
                textBox_Current.Text = "1";
                CurrentIndex = 1;
                trackBarControl1.Properties.Maximum = Total;
                trackBarControl1.Properties.Minimum = 1;
                trackBarControl1.Value = 1;
                trackBarControl1_ValueChanged(sender, e);
            }
            catch (Exception)
            {

                
            }
        }


        public string FindPoint_BF(int SideId, HObject IntesityImage, HObject HeightImage, out double[][] RowCoord, out double[][] ColCoord, out double[][] ZCoord, out string[][] StrLineOrCircle, HTuple HomMat3D = null, HWindow_Final hwind = null, bool debug = false, HTuple homMatFix = null)
        {
            //HObject RIntesity = new HObject(), RHeight = new HObject();
            StringBuilder Str = new StringBuilder();
            RowCoord = null; ColCoord = null; ZCoord = null; StrLineOrCircle = null;
            try
            {
                int Sid = SideId - 1;
                string ok1 = fpTool.GenProfileCoord(Sid + 1, HeightImage, out FindPointTool.RArray, out FindPointTool.Row, out FindPointTool.CArray, out FindPointTool.Phi, out Ignore, homMatFix);
                if (ok1 != "OK")
                {
                    return ok1;
                }
                int Len = fpTool.roiList2[Sid].Count;// 区域数量
                if (Len == 0)
                {

                    //RIntesity.Dispose();
                    //RHeight.Dispose();
                    return "参数设置错误";
                }
                RowCoord = new double[Len][]; ColCoord = new double[Len][]; ZCoord = new double[Len][]; StrLineOrCircle = new string[Len][];
                double[][] RowCoordt = new double[Len][]; double[][] ColCoordt = new double[Len][]; /*double[][] ZCoordt = new double[Len][];*/

                int Num = 0; int Add = 0;
                for (int i = 0; i < Len; i++)
                {

                    HTuple row = new HTuple(), col = new HTuple();
                    for (int j = Num; j < Add + fpTool.fParam[Sid].roiP[i].NumOfSection; j++)
                    {
                        if (Num == 99)
                        {
                            Debug.WriteLine(Num);
                        }
                        Debug.WriteLine(Num);
                        HTuple row1, col1; HTuple anchor, anchorc;
                        string ok = fpTool.FindMaxPt(Sid + 1, j, out row1, out col1, out anchor, out anchorc);
                        //if (ok!="OK")
                        //{
                        //    return ok;
                        //}

                        row = row.TupleConcat(row1);
                        col = col.TupleConcat(col1);
                        Num++;
                    }
                    Add = Num;
                    if (row.Length == 0) //区域之外
                    {
                        continue;
                    }
                    HObject siglePart = new HObject();
                    //直线段拟合
                    if (fpTool.fParam[Sid].roiP[i].LineOrCircle == "直线段")
                    {
                        HObject line = new HObject();
                        HOperatorSet.GenContourPolygonXld(out line, row, col);
                        HTuple Rowbg, Colbg, RowEd, ColEd, Nr, Nc, Dist;
                        HOperatorSet.FitLineContourXld(line, "tukey", -1, 0, 5, 2, out Rowbg, out Colbg, out RowEd, out ColEd, out Nr, out Nc, out Dist);
                        Rowbg = Rowbg + fpTool.fParam[Sid].roiP[i].offset;
                        Colbg = Colbg + fpTool.fParam[Sid].roiP[i].Xoffset;
                        RowEd = RowEd + fpTool.fParam[Sid].roiP[i].Yoffset2;
                        ColEd = ColEd + fpTool.fParam[Sid].roiP[i].Xoffset2;

                        row = (Rowbg.D + RowEd.D) / 2 + fpTool.fParam[Sid].roiP[i].offset;
                        col = (Colbg.D + ColEd.D) / 2 + fpTool.fParam[Sid].roiP[i].Xoffset;


                        HTuple linephi = new HTuple();
                        HOperatorSet.LineOrientation(Rowbg, Colbg, RowEd, ColEd, out linephi);
                        HTuple deg = -(new HTuple(linephi.D)).TupleDeg();
                        double tan = Math.Tan(-linephi.D);

                        int len1 = Math.Abs((int)(Rowbg.D - RowEd.D));
                        int len2 = Math.Abs((int)(Colbg.D - ColEd.D));
                        int len = len1 > len2 ? len1 : len2;
                        HTuple x0 = Colbg.D;
                        HTuple y0 = Rowbg.D;
                        double[] newr = new double[len]; double[] newc = new double[len];
                        for (int m = 0; m < len; m++)
                        {
                            HTuple row1 = new HTuple(); HTuple col1 = new HTuple();
                            if (len1 > len2)
                            {
                                row1 = Rowbg.D > RowEd.D ? y0 - m : y0 + m;
                                col1 = Rowbg.D > RowEd.D ? x0 - m / tan : x0 + m / tan;

                            }
                            else
                            {
                                row1 = Colbg.D > ColEd.D ? y0 - m * tan : y0 + m * tan;
                                col1 = Colbg.D > ColEd.D ? x0 - m : x0 + m;
                            }
                            newr[m] = row1;
                            newc[m] = col1;
                        }
                        row = newr; col = newc;

                        HOperatorSet.GenContourPolygonXld(out line, row, col);


                        ////两条线做判断
                        //if (Rowbg > RowEd)//从上往下 反向
                        //{
                        //    row = row.TupleSelectRange(row.Length / 2, row.Length - 1);
                        //    col = col.TupleSelectRange(col.Length / 2, col.Length - 1);
                        //}
                        //else
                        //{
                        //    row = row.TupleSelectRange(0, row.Length / 2);
                        //    col = col.TupleSelectRange(0, col.Length / 2);
                        //}

                        if (hwind != null && debug)
                        {
                            hwind.viewWindow.displayHobject(line, "red");
                        }
                        siglePart = line;
                    }
                    else
                    {
                        HObject ArcObj = new HObject();
                        HTuple tempR = new HTuple(row), tempC = new HTuple(col);
                        for (int n = 0; n < row.Length; n++)
                        {
                            if (n > 1)
                            {
                                double sub = (row[n].D - row[n - 1].D);
                                if (sub > 20)
                                {
                                    tempR[n] = tempR[n - 1].D + 1;
                                    tempC[n] = tempC[n - 1].D + 1;

                                }
                                if (sub < -20)
                                {
                                    tempR[n] = tempR[n - 1].D - 1;
                                    tempC[n] = tempC[n - 1].D + 1;

                                }
                            }
                        }
                        row = tempR; col = tempC;
                        HOperatorSet.GenContourPolygonXld(out ArcObj, row, col);

                        HOperatorSet.SmoothContoursXld(ArcObj, out ArcObj, 15);
                        if (fpTool.fParam[Sid].roiP[i].Xoffset2 != 0)
                        {
                            HTuple homMat;
                            HTuple deg1 = fpTool.fParam[Sid].roiP[i].Xoffset2;
                            HTuple Phi1 = deg1.TupleRad();
                            HOperatorSet.VectorAngleToRigid(row[0], col[0], 0, row[0], col[0], Phi1, out homMat);
                            HOperatorSet.AffineTransContourXld(ArcObj, out ArcObj, homMat);
                        }

                        HTuple Rb, Cb, Re, Ce, Nr1, Nc1, Ptorder;
                        HOperatorSet.FitLineContourXld(ArcObj, "tukey", -1, 0, 5, 2, out Rb, out Cb, out Re, out Ce, out Nr1, out Nc1, out Ptorder);

                        HOperatorSet.GetContourXld(ArcObj, out row, out col);

                        row = row + fpTool.fParam[Sid].roiP[i].offset;

                        col = col + fpTool.fParam[Sid].roiP[i].Xoffset;

                        if (hwind != null && debug)
                        {
                            hwind.viewWindow.displayHobject(ArcObj, "blue");
                        }
                        siglePart = ArcObj;
                    }




                    //加上 x y z 偏移
                    //row = row + fpTool.fParam[Sid].roiP[i].Yoffset;
                    //col = col + fpTool.fParam[Sid].roiP[i].Xoffset;


                    //旋转至原图
                    //HOperatorSet.AffineTransPoint2d(homMatRotateInvert, row, col, out row, out col);



                    HTuple AvraRow = new HTuple(), AvraCol = new HTuple(), AvraZ = new HTuple();
                    int PointCount = fpTool.fParam[Sid].roiP[i].NumOfSection;

                    //取Z值 
                    HTuple zcoord = new HTuple();
                    HOperatorSet.GetGrayval(HeightImage, row, col, out zcoord);


                    //除去 -100 的点
                    HTuple eq100 = new HTuple();
                    HOperatorSet.TupleLessElem(zcoord, -5, out eq100);
                    HTuple eq100Id = new HTuple();
                    HOperatorSet.TupleFind(eq100, 1, out eq100Id);
                    HTuple not5 = eq100.TupleFind(0);
                    HTuple Total = zcoord[not5];
                    HTuple Mean = Total.TupleMean();
                    if (eq100Id != -1)
                    {
                        for (int m = 0; m < eq100Id.Length; m++)
                        {
                            if (eq100Id[m] == 0)
                            {
                                HTuple meanZ = new HTuple();
                                meanZ = meanZ.TupleConcat(zcoord[eq100Id[m].D], zcoord[eq100Id[m].D + 1], zcoord[eq100Id[m].D + 2], zcoord[eq100Id[m].D + 3], zcoord[eq100Id[m].D + 4], zcoord[eq100Id[m].D + 5]);
                                HTuple eq30 = new HTuple();
                                HOperatorSet.TupleGreaterElem(meanZ, -5, out eq30);
                                HTuple eq30Id = new HTuple();
                                HOperatorSet.TupleFind(eq30, 1, out eq30Id);
                                if (eq30Id != -1)
                                {
                                    meanZ = meanZ[eq30Id];
                                    meanZ = meanZ.TupleMean();
                                    zcoord[eq100Id[m].D] = meanZ;
                                }
                                else
                                {
                                    meanZ = Mean;
                                    zcoord[eq100Id[m].D] = meanZ;
                                }

                            }
                            if (eq100Id[m] - 1 >= 0)
                            {
                                if (zcoord[eq100Id[m].D - 1].D < -5)
                                {
                                    zcoord[eq100Id[m].D] = Mean;
                                }
                                else
                                {
                                    zcoord[eq100Id[m].D] = zcoord[eq100Id[m].D - 1];
                                }

                            }
                            else
                            {
                                zcoord[eq100Id[m].D] = Mean;
                            }

                        }
                    }


                    //将行列Z 均分
                    double Div = row.Length / (double)((PointCount - 1));
                    if (Div < 1)
                    {
                        Div = 1;
                    }
                    for (int k = 0; k < PointCount; k++)
                    {
                        if (k == 0)
                        {
                            AvraRow = AvraRow.TupleConcat(row[0]);
                            AvraCol = AvraCol.TupleConcat(col[0]);
                            AvraZ = AvraZ.TupleConcat(zcoord[0]);
                            continue;
                        }

                        if (k == 529)
                        {
                            Debug.WriteLine("k" + k);
                        }
                        //Debug.WriteLine("k" + k);


                        int id = (int)(Div * k);
                        if (id >= row.Length)
                        {
                            break;
                        }
                        AvraRow = AvraRow.TupleConcat(row[id]);
                        AvraCol = AvraCol.TupleConcat(col[id]);
                        AvraZ = AvraZ.TupleConcat(zcoord[id]);
                    }
                    row = AvraRow;
                    col = AvraCol;
                    zcoord = AvraZ;


                    HTuple xc = new HTuple();
                    HOperatorSet.TupleGenSequence(0, zcoord.Length - 1, 1, out xc);
                    HObject Cont = new HObject();
                    HOperatorSet.GenContourPolygonXld(out Cont, zcoord, xc);
                    if (fpTool.fParam[Sid].roiP[i].LineOrCircle == "直线段")
                    {

                        HTuple Rowbg1, Colbg1, RowEd1, ColEd1, Nr1, Nc1, Dist1;
                        HOperatorSet.FitLineContourXld(Cont, "tukey", -1, 0, 5, 2, out Rowbg1, out Colbg1, out RowEd1, out ColEd1, out Nr1, out Nc1, out Dist1);

                        HTuple linephi1 = new HTuple();
                        HOperatorSet.LineOrientation(Rowbg1, Colbg1, RowEd1, ColEd1, out linephi1);
                        //HTuple deg = -(new HTuple(linephi1.D)).TupleDeg();
                        double tan1 = Math.Tan(-linephi1.D);

                        int len11 = Math.Abs((int)(Rowbg1.D - RowEd1.D));
                        int len21 = Math.Abs((int)(Colbg1.D - ColEd1.D));
                        int len0 = zcoord.Length;
                        HTuple x01 = Colbg1.D;
                        HTuple y01 = Rowbg1.D;
                        double[] newr1 = new double[len0]; double[] newc1 = new double[len0];
                        for (int m = 0; m < len0; m++)
                        {
                            HTuple row1 = new HTuple(); HTuple col1 = new HTuple();
                            if (len11 > len21)
                            {
                                row1 = Rowbg1.D > RowEd1.D ? y01 - m : y01 + m;
                                col1 = Rowbg1.D > RowEd1.D ? x01 - m / tan1 : x01 + m / tan1;

                            }
                            else
                            {
                                row1 = Colbg1.D > ColEd1.D ? y01 - m * tan1 : y01 + m * tan1;
                                col1 = Colbg1.D > ColEd1.D ? x01 - m : x01 + m;
                            }
                            newr1[m] = row1;
                            newc1[m] = col1;
                        }
                        zcoord = newr1; xc = newc1;

                    }
                    else
                    {
                        HOperatorSet.GenContourPolygonXld(out Cont, xc, zcoord);
                        HOperatorSet.SmoothContoursXld(Cont, out Cont, 15);
                        HOperatorSet.GetContourXld(Cont, out xc, out zcoord);
                    }


                    HTuple origRow = row;
                    HTuple origCol = col;

                    if (true)

                        #region 单边去重
                        if (i > 0)
                        {
                            HObject regXld = new HObject();
                            HOperatorSet.GenRegionContourXld(siglePart, out regXld, "filled");
                            HTuple phi;
                            HOperatorSet.RegionFeatures(regXld, "phi", out phi);
                            phi = phi.TupleDeg();
                            phi = phi.TupleAbs();
                            //HTuple last1 = RowCoordt[i - 1][0];//x1
                            //HTuple lastc1 = ColCoordt[i - 1][0];//x1
                            //HTuple sub1 = Math.Abs(row[0].D - last1.D);
                            //HTuple sub2 = Math.Abs(col[0].D - lastc1.D);
                            //HTuple pt1 = sub1.D > sub2.D ? RowCoordt[i - 1][RowCoordt[i - 1].Length - 1] : ColCoordt[i - 1][ColCoordt[i - 1].Length - 1];
                            HTuple pt1 = phi.D > 75 ? RowCoordt[i - 1][RowCoordt[i - 1].Length - 1] : ColCoordt[i - 1][ColCoordt[i - 1].Length - 1];
                            HTuple colbase = Sid == 0 || Sid == 2 ? col.TupleLessEqualElem(pt1) : col.TupleGreaterEqualElem(pt1);
                            HTuple Grater1 = phi.D > 75 ? row.TupleGreaterEqualElem(pt1) : colbase;

                            switch (Sid)
                            {
                                case 0: //x2>x1

                                    HTuple Grater1id = Grater1.TupleFind(1);
                                    row = row.TupleRemove(Grater1id);
                                    col = col.TupleRemove(Grater1id);
                                    zcoord = zcoord.TupleRemove(Grater1id);

                                    break;
                                case 1: //y2>y1

                                    HTuple Grater2id = Grater1.TupleFind(1);
                                    row = row.TupleRemove(Grater2id);
                                    col = col.TupleRemove(Grater2id);
                                    //origRow = origRow.TupleRemove(Grater2id);
                                    //origCol = origCol.TupleRemove(Grater2id);
                                    zcoord = zcoord.TupleRemove(Grater2id);
                                    break;
                                case 2: //x2<x1

                                    HTuple Grater3id = Grater1.TupleFind(1);
                                    row = row.TupleRemove(Grater3id);
                                    col = col.TupleRemove(Grater3id);
                                    //origRow = origRow.TupleRemove(Grater3id);
                                    //origCol = origCol.TupleRemove(Grater3id);
                                    zcoord = zcoord.TupleRemove(Grater3id);
                                    break;
                                case 3: //y2<y1

                                    HTuple Grater4id = Grater1.TupleFind(1);
                                    row = row.TupleRemove(Grater4id);
                                    col = col.TupleRemove(Grater4id);
                                    //origRow = origRow.TupleRemove(Grater4id);
                                    //origCol = origCol.TupleRemove(Grater4id);
                                    zcoord = zcoord.TupleRemove(Grater4id);
                                    break;
                            }
                        }
                    #endregion 单边去重


                    if (row.Length == 0)
                    {
                        return "单边设置 区域重复点数过大";
                    }

                    if (hwind != null && debug == false)
                    {
                        HObject NewSide = new HObject();
                        HOperatorSet.GenContourPolygonXld(out NewSide, row, col);
                        hwind.viewWindow.displayHobject(NewSide, "red");
                    }

                    RowCoordt[i] = row;
                    ColCoordt[i] = col;
                    //进行矩阵变换
                    if (HomMat3D != null)
                    {
                        //HOperatorSet.AffineTransPoint3d(HomMat3D, row, col,zcoord, out row, out col,out zcoord);
                        HOperatorSet.AffineTransPoint2d(HomMat3D, row, col, out row, out col);
                        //zcoord = zcoord + fpTool.fParam[Sid].roiP[i].Zoffset + fpTool.fParam[Sid].SigleZoffset + MyGlobal.globalConfig.TotalZoffset;
                    }

                    for (int n = 0; n < row.Length; n++)
                    {
                        Str.Append(row[n].D.ToString() + "," + col[n].D.ToString() + "," + zcoord[n].D.ToString() + "\r\n");
                    }
                    RowCoord[i] = row;
                    ColCoord[i] = col;
                    ZCoord[i] = zcoord;
                    StrLineOrCircle[i] = new string[zcoord.Length];


                    //ColCoordt[i] = col;
                    //ZCoordt[i] = zcoord;


                    StrLineOrCircle[i][0] = fpTool.fParam[Sid].roiP[i].LineOrCircle == "直线段" ? "1;" : "2;";
                    for (int n = 1; n < zcoord.Length; n++)
                    {

                        if (n == zcoord.Length - 1 && i == Len - 1 && Sid != 3) //最后一段 第四边不给
                        {
                            StrLineOrCircle[i][n] = StrLineOrCircle[i][0];
                        }
                        else
                        {
                            StrLineOrCircle[i][n] = "0;";

                        }
                    }
                    //if (hwind != null && debug == false)
                    //{
                    //    HObject NewSide = new HObject();
                    //    HOperatorSet.GenContourPolygonXld(out NewSide, row, col);
                    //    hwind.viewWindow.displayHobject(NewSide, "red");
                    //}

                }


                if (HomMat3D == null)
                {
                    StaticOperate.writeTxt("D:\\Laser3D.txt", Str.ToString());
                }

                //RIntesity.Dispose();
                //RHeight.Dispose();
                return "OK";
            }
            catch (Exception ex)
            {

                //RIntesity.Dispose();
                //RHeight.Dispose();
                return "FindPoint error:" + ex.Message;
            }
        }



        private void 更改ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                int Id = Convert.ToInt32(SideName.Substring(4, 1)) - 1;
                if (dataGridView1.CurrentCell == null)
                {
                    return;
                }
                if (fpTool.roiList2[Id].Count > 0 && roiController2.ROIList.Count == fpTool.roiList2[Id].Count)
                {
                    int roiID = dataGridView1.CurrentCell.RowIndex;
                    roiID = CurrentRowIndex;
                    if (roiID < 0)
                    {
                        return;
                    }
                    RoiIsMoving = true;
                    if (roiID >= 0)
                    {

                        //fpTool.fParam[Id].roiP[roiId].LineOrCircle = fpTool.fParam[Id].roiP[roiId].LineOrCircle == "直线段" ? "圆弧段" : "直线段";
                        //comboBox2.SelectedItem = fpTool.fParam[Id].roiP[roiId].LineOrCircle;
                        switch (fpTool.fParam[Id].roiP[roiID].LineOrCircle)
                        {
                            case "直线段":
                                if (MessageBox.Show("更改为圆弧段(是)更改为连接段(否)", "提示", MessageBoxButtons.YesNo) == DialogResult.Yes)
                                {
                                    fpTool.fParam[Id].roiP[roiID].LineOrCircle = "圆弧段";
                                }
                                else
                                {
                                    fpTool.fParam[Id].roiP[roiID].LineOrCircle = "连接段";
                                }
                                break;
                            case "圆弧段":
                                if (MessageBox.Show("更改为直线段(是)更改为连接段(否)", "提示", MessageBoxButtons.YesNo) == DialogResult.Yes)
                                {
                                    fpTool.fParam[Id].roiP[roiID].LineOrCircle = "直线段";
                                }
                                else
                                {
                                    fpTool.fParam[Id].roiP[roiID].LineOrCircle = "连接段";
                                }
                                break;
                            case "连接段":
                                if (MessageBox.Show("更改为直线段(是)更改为圆弧段(否)", "提示", MessageBoxButtons.YesNo) == DialogResult.Yes)
                                {
                                    fpTool.fParam[Id].roiP[roiID].LineOrCircle = "直线段";
                                }
                                else
                                {
                                    fpTool.fParam[Id].roiP[roiID].LineOrCircle = "圆弧段";
                                }
                                break;
                        }
                        comboBox2.SelectedItem = fpTool.fParam[Id].roiP[roiID].LineOrCircle;
                        dataGridView1.Rows[roiID].Cells[2].Value = fpTool.fParam[Id].roiP[roiID].LineOrCircle;

                    }
                    RoiIsMoving = false;
                }
            }
            catch (Exception)
            {

                throw;
            }
        }

        private void FitLineSet_FormClosing(object sender, FormClosingEventArgs e)
        {

            if (isSave)
            {
              
            }
            else
            {
                DialogResult result = MessageBox.Show("当前参数未保存，是否关闭?", "提示：", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

                if (result != DialogResult.Yes)
                {
                    e.Cancel = true;
                    return;
                }
            }

            RoiParam.isInvoke = false;
            RoiParam.ChangeSection -= RoiParam_ChangeSection;
            //MyGlobal.Right_findPointTool_Find.Init("FitLineSet", isRight);
            //MyGlobal.Left_findPointTool_Find.Init("FitLineSet", isRight);
            //MyGlobal.Right_findPointTool_Fix.Init("Fix", isRight);
            //MyGlobal.Left_findPointTool_Fix.Init("Fix", isRight);

            //MyGlobal.flset2.Init();
            if (hwindow_final1.Image != null)
            {
                hwindow_final1.Image.Dispose();
                hwindow_final1.ClearWindow();
            }
            if (hwindow_final2.Image != null)
            {
                hwindow_final2.Image.Dispose();
                hwindow_final2.ClearWindow();
            }

            RGBImage.Dispose();
            checkBox1.Checked = false;
            checkBox2.Checked = false;
            checkBox4.Checked = false;
            checkBoxRoi.Checked = false;
            //comboBox1.SelectedIndex = 0;
            //comboBox2.SelectedIndex = 0;
            isCloing = true;
            CurrentSide = "Side1";
        }
        bool isInsert = false;
        private void 插入ToolStripMenuItem_Click(object sender, EventArgs e)
        {
           
            isGenSection = false;
            if (dataGridView1.CurrentCell == null)
            {
                return;
            }
            int Id = Convert.ToInt32(SideName.Substring(4, 1)) - 1;



            if (hwindow_final2.Image == null || !hwindow_final2.Image.IsInitialized())
            {
                return;
            }

            int currentId = dataGridView1.CurrentCell.RowIndex;
            currentId = CurrentRowIndex;
            if (currentId < 0)
            {
                return;
            }
            RoiIsMoving = true;
            isInsert = true;
            HTuple coord1 = fpTool.roiList[Id][currentId].getModelData();
            HTuple coord2 = fpTool.roiList2[Id][currentId].getModelData();
           

            List<ROI> roiListTemp = new List<ROI>();
            List<ROI> roiListTemp1 = new List<ROI>();
            
             HWindow_Final tempWindow = new HWindow_Final();
            tempWindow.viewWindow.genRect2(coord2[0].D + 45, coord2[1].D + 45, coord2[2].D, coord2[3].D, coord2[4].D, ref roiListTemp);
            ROI temp2 = roiListTemp[0];
            tempWindow.viewWindow.genRect1(coord1[0].D, coord1[1].D, coord1[2].D, coord1[3].D, ref roiListTemp1);
            ROI rec = roiListTemp1[0];
            if (this.Text == "Detect")
            {
                List<ROI> roiListTemp_Detect = new List<ROI>();
                HTuple coord3 = fpTool.DetectRoiList[Id][currentId].getModelData();
                tempWindow.viewWindow.genRect1(coord3[0].D, coord3[1].D, coord3[2].D, coord3[3].D, ref roiListTemp_Detect);
                fpTool.DetectRoiList[Id].Insert(currentId, roiListTemp_Detect[0]);
                hwindow_final3.viewWindow.notDisplayRoi();
                if (fpTool.DetectRoiList[Id].Count > 0)
                {
                    temp_Detect.Clear();
                    ROI roi = fpTool.DetectRoiList[Id][currentId + 1];
                    temp_Detect.Add(roi);
                    hwindow_final3.viewWindow.notDisplayRoi();
                    hwindow_final3.viewWindow.displayROI(ref temp_Detect);
                }
            }

            fpTool.roiList2[Id].Insert(currentId, temp2);
            fpTool.roiList[Id].Insert(currentId, rec);

            hwindow_final1.viewWindow.notDisplayRoi();
            hwindow_final2.viewWindow.notDisplayRoi();

            if (fpTool.roiList2[Id].Count > 0)
            {
                if (SelectAll)
                {
                    hwindow_final2.viewWindow.notDisplayRoi();
                    roiController2.viewController.ShowAllRoiModel = -1;
                    hwindow_final2.viewWindow.displayROI(ref fpTool.roiList2[Id]);
                }
                else
                {
                    hwindow_final2.viewWindow.displayROI(ref fpTool.roiList2[Id]);
                    roiController2.viewController.ShowAllRoiModel = currentId + 1;
                    roiController2.viewController.repaint(currentId + 1);
                }
                hwindow_final2.viewWindow.setActiveRoi(currentId + 1);
            }
            if (fpTool.roiList[Id].Count > 0)
            {
                temp.Clear();
                ROI roi = fpTool.roiList[Id][currentId + 1];
                temp.Add(roi);
                hwindow_final1.viewWindow.notDisplayRoi();
                hwindow_final1.viewWindow.displayROI(ref temp);
            }

            RoiParam RP = new RoiParam();
            RP = fpTool.fParam[Id].roiP[currentId].Clone();
            //新位置的
            HTuple NewCoord = temp2.getModelData();
            RP.CenterRow = NewCoord[0]; RP.CenterCol = NewCoord[1];
            fpTool.fParam[Id].roiP.Insert(currentId, RP);
            fpTool.fParam[Id].roiP[currentId].LineOrCircle = comboBox2.SelectedItem.ToString();
            //insert
            dataGridView1.Rows.Insert(currentId);
            dataGridView1.Rows[currentId].Cells[0].Value = currentId;
            dataGridView1.Rows[currentId].Cells[1].Value = "Default";
            dataGridView1.Rows[currentId].Cells[2].Value = fpTool.fParam[Id].roiP[currentId].LineOrCircle;
            fpTool.fParam[Id].DicPointName.Insert(currentId, "Default");
            //排序
            for (int i = 0; i < dataGridView1.Rows.Count; i++)
            {
                dataGridView1.Rows[i].Cells[0].Value = i;
            }

            //roiController2.setROIShape(new ROIRectangle2());
            RoiIsMoving = false;
        }

        int PtOrder = -1;
        private void simpleButton5_Click_1(object sender, EventArgs e)//偏移起终点1，2
        {
            if (hwindow_final1.Image == null || !hwindow_final1.Image.IsInitialized())
            {
                return;
            }
            SimpleButton sbtn = (SimpleButton)sender;
            hwindow_final1.viewWindow.notDisplayRoi();
            switch (sbtn.Text)
            {
                case "偏移起点1":
                    PtOrder = 0;

                    roiController.setROIShape(new ROIPoint());
                    break;
                case "偏移终点1":
                    PtOrder = 1;
                    roiController.setROIShape(new ROIPoint());
                    break;
                case "偏移起点2":
                    PtOrder = 2;
                    roiController.setROIShape(new ROIPoint());
                    break;
                case "偏移终点2":
                    PtOrder = 3;
                    roiController.setROIShape(new ROIPoint());
                    break;
            }
        }

        bool edit = false;
        private void checkBoxRoi_CheckedChanged(object sender, EventArgs e)//ROI编辑模式
        {
            edit = checkBoxRoi.Checked;
            if (edit)
            {
                hwindow_final1.viewWindow.setEditModel(true);
                hwindow_final2.viewWindow.setEditModel(true);
            }
            else
            {
                hwindow_final1.viewWindow.setEditModel(false);
                hwindow_final2.viewWindow.setEditModel(false);
            }
        }
        bool ReName = false;
       


        bool isSelectOne = false;
        string PreSelect = "";

        private void textBox_OffsetX_TextChanged(object sender, EventArgs e)
        {
            try
            {
                if (isLoading)
                {
                    return;
                }

                int SideId = Convert.ToInt32(SideName.Substring(4, 1)) - 1;
                TextBox tb = (TextBox)sender;
                string index = tb.Text.ToString();
                //bool ok = Regex.IsMatch(index, @"(?i)^(\-[0-9]{1,}[.][0-9]*)+$") || Regex.IsMatch(index, @"(?i)^(\-[0-9]{1,}[0-9]*)+$") || Regex.IsMatch(index, @"(?i)^([0-9]{1,}[0-9]*)+$") || Regex.IsMatch(index, @"(?i)^(\[0-9]{1,}[0-9]*)+$");
                bool ok = Regex.IsMatch(index, @"^[-]?\d+[.]?\d*$");//是否为数字
                //bool ok = Regex.IsMatch(index, @"^([-]?)\d*$");//是否为整数
                if (!ok || RoiIsMoving)
                {
                    return;
                }
                double num = double.Parse(index);
                int roiID = dataGridView1.CurrentCell.RowIndex;
                roiID = CurrentRowIndex;
                if (roiID == -1)
                {
                    return;
                }
                int count = dataGridView1.SelectedCells.Count;
                List<int> rowInd = new List<int>();
                for (int i = 0; i < count; i++)
                {
                    int Ind = dataGridView1.SelectedCells[i].RowIndex;
                    if (!rowInd.Contains(Ind))
                    {
                        rowInd.Add(Ind);
                    }
                }
                for (int i = 0; i < rowInd.Count; i++)
                {
                    switch (tb.Name)
                    {
                        case "textBox_OffsetX":
                            fpTool.fParam[SideId].roiP[rowInd[i]].Xoffset = num;
                            break;
                        case "textBox_OffsetY":

                            fpTool.fParam[SideId].roiP[rowInd[i]].Yoffset = num;
                            break;
                        case "textBox_Offset":

                            fpTool.fParam[SideId].roiP[rowInd[i]].offset = num;
                            break;
                        case "textBox_ZFtMax":
                            fpTool.fParam[SideId].roiP[rowInd[i]].ZftMax = (int)num;
                            break;
                        case "textBox_ZFtMin":
                            fpTool.fParam[SideId].roiP[rowInd[i]].ZftMin = (int)num;
                            break;
                        case "textBox_ZFtRad":
                            fpTool.fParam[SideId].roiP[rowInd[i]].ZftRad = num;
                            break;
                        case "textBox_downDist":
                            fpTool.fParam[SideId].roiP[rowInd[i]].TopDownDist = num;
                            break;
                        case "textBox_xDist":
                            fpTool.fParam[SideId].roiP[rowInd[i]].xDist = num;
                            break;
                        case "textBox_Clipping":
                            //if (num > 50)
                            //{
                            //    textBox_Clipping.Text = "50";
                            //    num = 50;
                            //}
                            //if (num < 0)
                            //{
                            //    textBox_Clipping.Text = "0";
                            //    num = 0;
                            //}
                            fpTool.fParam[SideId].roiP[rowInd[i]].ClippingPer = num;
                            break;

                        case "textBox_SmoothCont":
                            //if (num > 1)
                            //{
                            //    textBox_SmoothCont.Text = "1";
                            //    num = 50;
                            //}
                            //if (num < 0)
                            //{
                            //    textBox_SmoothCont.Text = "0";
                            //    num = 0;
                            //}
                            fpTool.fParam[SideId].roiP[rowInd[i]].SmoothCont = num;
                            break;
                        case "textBox_OffsetZ":
                            fpTool.fParam[SideId].roiP[rowInd[i]].Zoffset = num;
                            break;
                        case "textBox_IndStart1":
                            //fpTool.fParam[SideId].roiP[roiID].StartOffSet1 = (int)num;
                            break;
                        case "textBox_IndEnd1":
                            //fpTool.fParam[SideId].roiP[roiID].EndOffSet1 = (int)num;
                            break;
                        case "textBox_IndStart2":
                            //fpTool.fParam[SideId].roiP[roiID].StartOffSet2 = (int)num;
                            break;
                        case "textBox_IndEnd2":
                            //fpTool.fParam[SideId].roiP[roiID].EndOffSet2 = (int)num;
                            break;
                    }
                }

            }

            catch (Exception)
            {

                throw;
            }
        }

        private void checkBox4_CheckedChanged(object sender, EventArgs e)//不启用定位
        {
            NotUseFix = checkBox4.Checked;
            RoiParam.isInvoke = false;
            LoadToUI();
            ChangeSide();
            RoiParam.isInvoke = true;
        }

     

        bool isSelecting = false;
        private void dataGridView1_CurrentCellChanged(object sender, EventArgs e)
        {

            if (dataGridView1.CurrentCell == null || RoiIsMoving || isSelecting || isLoading)
            {
                return;
            }
            int roiID = dataGridView1.CurrentCell.RowIndex;
            if (roiID < 0)
            {
                return;
            }
            isSelecting = true;
            if (SelectAll)
            {

            }
            else
            {
                dataGridView1.ClearSelection();
                dataGridView1.Rows[roiID].Selected = true;
            }

            isSelecting = false;
            try
            {
                isGenSection = false;
                RoiIsMoving = true;
                isSelectOne = false;
                int SideId = Convert.ToInt32(SideName.Substring(4, 1)) - 1;

                int id = roiID;

                //hwindow_final2.viewWindow.notDisplayRoi();
                if (fpTool.roiList2[SideId].Count > 0)
                {
                    if (SelectAll)
                    {
                        hwindow_final2.viewWindow.notDisplayRoi();
                        roiController2.viewController.ShowAllRoiModel = -1;
                        hwindow_final2.viewWindow.displayROI(ref fpTool.roiList2[SideId]);
                        hwindow_final2.viewWindow.selectROI(id);
                    }
                    else
                    {
                        if (roiController2.ROIList.Count != fpTool.roiList2[SideId].Count)
                        {
                            roiController2.viewController.ShowAllRoiModel = -1;
                            hwindow_final2.viewWindow.displayROI(ref fpTool.roiList2[SideId]);
                        }
                        //hwindow_final2.viewWindow.notDisplayRoi();
                        //hwindow_final2.viewWindow.displayROI(ref fpTool.roiList2[SideId]);
                        roiController2.viewController.ShowAllRoiModel = id;
                        roiController2.viewController.repaint(id);
                    }

                }
                else
                {
                    return;
                }

                HTuple[] lineCoord = new HTuple[1];

               
                fpTool.DispSection((ROIRectangle2)fpTool.roiList2[SideId][id], SideId, id, out lineCoord, hwindow_final2);

             
                int pID = 1;
                for (int i = 0; i < id; i++)
                {
                    for (int j = 0; j < fpTool.fParam[SideId].roiP[i].NumOfSection; j++)
                    {
                        pID++;
                    }
                }

                CurrentIndex = pID;
                CurrentRowIndex = roiID;
                textBox_Current.Text = CurrentIndex.ToString();

                int CurrentRoiId = 0;
                int n = 0;
                for (int i = 0; i < roiID; i++)
                {
                    n = n + fpTool.fParam[SideId].roiP[i].NumOfSection;
                }
                CurrentRoiId = CurrentIndex - n;

                if (FindType == MyGlobal.FindPointType_Detect)
                {
                    if (pResult.Row != null)
                    {
                        string ok2 = fpTool.FindPoint_Detect(SideId, HeightImage, out pResult, null, hwindow_final2, hwindow_final1, ShowSection, roiID + 1, CurrentRoiId);
                        if (ok2 != "OK")
                        {
                            MessageBox.Show(ok2);
                        }
                        if (this.Text == "Detect" && GlueHeightImage != null && GlueHeightImage.IsInitialized())
                        {
                            ProfileResult GlueResult = new ProfileResult();
                            ok2 = fpTool.FindPoint_Detect(SideId , GlueHeightImage, out GlueResult, null, null, null, ShowSection, roiID + 1, CurrentRoiId);
                            HObject reg;
                            HOperatorSet.GenRegionPoints(out reg, new HTuple(GlueResult.ZCoord[CurrentIndex - 1]), new HTuple(GlueResult.ProfileCol[CurrentIndex - 1]));
                            hwindow_final1.viewWindow.displayHobject(reg, "green");
                            reg.Dispose();
                            DetectResult detectRst;
                            fpTool.SubProfile(pResult, GlueResult, SideId, roiID , CurrentRoiId - 1,out detectRst, hwindow_final3);
                            ShowDetectResult(detectRst);
                        }
                    }

                }
                else
                {
                    HTuple row, col; HTuple anchor, anchorc;
                    if (fpTool.fParam[SideId].roiP[CurrentRowIndex].SelectedType == 0)
                    {
                        if (fpTool.fParam[SideId].roiP[CurrentRowIndex].TopDownDist != 0 && fpTool.fParam[SideId].roiP[CurrentRowIndex].xDist != 0)
                        {
                            fpTool.FindMaxPtFallDown(SideId + 1, CurrentIndex - 1, out row, out col, out anchor, out anchorc, hwindow_final1, ShowSection, false, hwindow_final2);

                        }
                        else
                        {
                            fpTool.FindMaxPt(SideId + 1, CurrentIndex - 1, out row, out col, out anchor, out anchorc, hwindow_final2, ShowSection, false, hwindow_final1);

                        }
                    }
                    else
                    {
                        fpTool.FindMaxPtFallDown(SideId + 1, CurrentIndex - 1, out row, out col, out anchor, out anchorc, hwindow_final1, ShowSection, false, hwindow_final2);
                    }
                }


                temp.Clear();
                ROI roi = fpTool.roiList[SideId][id];
                temp.Add(roi);
                hwindow_final1.viewWindow.notDisplayRoi();
                hwindow_final1.viewWindow.displayROI(ref temp);


                propertyGrid1.SelectedObject = fpTool.fParam[SideId].roiP[id];


                if (this.Text == "Detect")
                {
                    temp_Detect.Clear();
                    ROI roi_detect = fpTool.DetectRoiList[SideId][id];
                    temp_Detect.Add(roi_detect);
                    hwindow_final3.viewWindow.notDisplayRoi();
                    hwindow_final3.viewWindow.displayROI(ref temp_Detect);
                    ChangeDetectParam(SideId, id);
                
                    hwindow_final2.viewWindow.saveROI(fpTool.DetectRoiList[id], ParamPath.ParamDir + SideName + "_detect.roi");
                    StaticOperate.WriteXML(fpTool.detectParam[id], ParamPath.ParamDir + SideName + "_DetectParam.xml");
                }

                RoiIsMoving = false;
            }
            catch (Exception)
            {
                RoiIsMoving = false;
                //Debug.WriteLine(richTextBox1.SelectedText);
           
            }
        }

        public void ChangeDetectParam(int SideId, int roiId)
        {
            if (fpTool.detectParam[SideId] == null )
            {
                fpTool.detectParam[SideId] = new List<DetectParam>();
            }
            if (fpTool.detectParam[SideId].Count <= roiId)
            {
                fpTool.detectParam[SideId].Add(new DetectParam());
            }
            dataGridView3.Rows[0].Cells[2].Value = fpTool.detectParam[SideId][roiId].MinWidth;
            dataGridView3.Rows[1].Cells[2].Value = fpTool.detectParam[SideId][roiId].MinHeight;
            dataGridView3.Rows[2].Cells[2].Value = fpTool.detectParam[SideId][roiId].MinArea;
            dataGridView3.Rows[3].Cells[2].Value = fpTool.detectParam[SideId][roiId].MinInnerGap;
            dataGridView3.Rows[4].Cells[2].Value = fpTool.detectParam[SideId][roiId].MinOuterGap;
            dataGridView3.Rows[0].Cells[3].Value = fpTool.detectParam[SideId][roiId].MaxWidth;
            dataGridView3.Rows[1].Cells[3].Value = fpTool.detectParam[SideId][roiId].MaxHeight;
            dataGridView3.Rows[2].Cells[3].Value = fpTool.detectParam[SideId][roiId].MaxArea;
            dataGridView3.Rows[3].Cells[3].Value = fpTool.detectParam[SideId][roiId].MaxInnerGap;
            dataGridView3.Rows[4].Cells[3].Value = fpTool.detectParam[SideId][roiId].MaxOuterGap;
        }
        public void ShowDetectResult(DetectResult detectRst)
        {
            dataGridView3.Rows[0].Cells[1].Value = detectRst.width;
            dataGridView3.Rows[1].Cells[1].Value = detectRst.height;
            dataGridView3.Rows[2].Cells[1].Value = detectRst.area;
            dataGridView3.Rows[3].Cells[1].Value = detectRst.innerGap;
            dataGridView3.Rows[4].Cells[1].Value = detectRst.outerGap;
        }

        ROI CopyTemp;
        ROI CopyTemp2;
        ROI CopyTemp_detect;
        int CopyId = -1;
        int CurrentRowIndex = -1;
        private void 复制ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                isGenSection = false;
                if (dataGridView1.CurrentCell == null)
                {
                    return;
                }
                int Id = Convert.ToInt32(SideName.Substring(4, 1)) - 1;

                if (hwindow_final2.Image == null || !hwindow_final2.Image.IsInitialized())
                {
                    return;
                }

                int currentId = CurrentRowIndex;
                if (currentId < 0)
                {
                    return;
                }

                HTuple coord1 = fpTool.roiList[Id][currentId].getModelData();
                HTuple coord2 = fpTool.roiList2[Id][currentId].getModelData();

                List<ROI> roiListTemp = new List<ROI>();
                List<ROI> roiListTemp1 = new List<ROI>();
                HWindow_Final tempWindow = new HWindow_Final();
                tempWindow.viewWindow.genRect2(coord2[0].D + 45, coord2[1].D + 45, coord2[2].D, coord2[3].D, coord2[4].D, ref roiListTemp);
                CopyTemp2 = roiListTemp[0];
                tempWindow.viewWindow.genRect1(coord1[0].D, coord1[1].D, coord1[2].D, coord1[3].D, ref roiListTemp1);
                CopyTemp = roiListTemp1[0];

                if (this.Text == "Detect")
                {
                    HTuple coord3 = fpTool.DetectRoiList[Id][currentId].getModelData();
                    List<ROI> roiList_detect = new List<ROI>();
                    tempWindow.viewWindow.genRect1(coord3[0].D, coord3[1].D, coord3[2].D, coord3[3].D, ref roiList_detect);
                    CopyTemp_detect = roiList_detect[0];
                }

                CopyId = currentId;
            }
            catch (Exception)
            {

                throw;
            }

        }

        private void 粘贴ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                isGenSection = false;
                if (CopyId == -1)
                {
                    return;
                }
                RoiIsMoving = true;
                int Id = Convert.ToInt32(SideName.Substring(4, 1)) - 1;
                int currentId = dataGridView1.CurrentCell.RowIndex;
                currentId = CurrentRowIndex;
                //添加到当前Id之后
                fpTool.roiList2[Id].Insert(currentId + 1, CopyTemp2);
                fpTool.roiList[Id].Insert(currentId + 1, CopyTemp);

                hwindow_final1.viewWindow.notDisplayRoi();
                hwindow_final2.viewWindow.notDisplayRoi();
                if (fpTool.roiList2[Id].Count > 0)
                {
                    if (SelectAll)
                    {
                        //hwindow_final2.viewWindow.notDisplayRoi();
                        roiController2.viewController.ShowAllRoiModel = -1;
                        hwindow_final2.viewWindow.displayROI(ref fpTool.roiList2[Id]);
                    }
                    else
                    {
                        roiController2.viewController.ShowAllRoiModel = currentId + 1;
                        hwindow_final2.viewWindow.displayROI(ref fpTool.roiList2[Id]);                       
                        roiController2.viewController.repaint(currentId + 1);
                    }
                    hwindow_final2.viewWindow.setActiveRoi(currentId + 1);
                }
                if (fpTool.roiList[Id].Count > 0)
                {
                    temp.Clear();
                    ROI roi = fpTool.roiList[Id][CopyId + 1];
                    temp.Add(roi);
                    hwindow_final1.viewWindow.notDisplayRoi();
                    hwindow_final1.viewWindow.displayROI(ref temp);
                }
                if (this.Text == "Detect")
                {
                    if (fpTool.DetectRoiList[Id].Count > 0)
                    {
                        temp_Detect.Clear();
                        ROI roi = fpTool.DetectRoiList[Id][CopyId + 1];
                        temp_Detect.Add(roi);
                        hwindow_final3.viewWindow.notDisplayRoi();
                        hwindow_final3.viewWindow.displayROI(ref temp);
                    }
                }

                RoiParam RP = new RoiParam();
                RP = fpTool.fParam[Id].roiP[CopyId].Clone();
                //新位置的
                HTuple NewCoord = CopyTemp2.getModelData();
                RP.CenterRow = NewCoord[0]; RP.CenterCol = NewCoord[1];
                fpTool.fParam[Id].roiP.Insert(currentId + 1, RP);
                fpTool.fParam[Id].roiP[currentId + 1].LineOrCircle = RP.LineOrCircle;
                //insert
                dataGridView1.Rows.Insert(currentId + 1);
                dataGridView1.Rows[currentId + 1].Cells[0].Value = currentId + 1;
                dataGridView1.Rows[currentId + 1].Cells[1].Value = "Default";
                dataGridView1.Rows[currentId + 1].Cells[2].Value = fpTool.fParam[Id].roiP[currentId + 1].LineOrCircle;
                fpTool.fParam[Id].DicPointName.Insert(currentId + 1, "Default");
                //排序
                for (int i = 0; i < dataGridView1.Rows.Count; i++)
                {
                    dataGridView1.Rows[i].Cells[0].Value = (i + 1);
                }
                dataGridView1.ClearSelection();
                dataGridView1.Rows[currentId + 1].Selected = true;
                CurrentRowIndex = currentId + 1;
                RoiIsMoving = false;
            }
            catch (Exception)
            {

                //throw;
            }
        }

        private void dataGridView1_CellValueChanged(object sender, DataGridViewCellEventArgs e)
        {
            try
            {
                if (isLoading)
                {
                    return;
                }
                bool newID = dataGridView1.IsCurrentCellInEditMode;
                if (!newID)
                {
                    return;
                }
                int Id = Convert.ToInt32(SideName.Substring(4, 1)) - 1;
                int currentId = CurrentRowIndex;
                if (currentId != -1)
                {
                    if (dataGridView1.Rows[currentId].Cells[1] == null)
                    {
                        return;
                    }
                    string Value = dataGridView1.Rows[currentId].Cells[1].Value.ToString();
                    if (fpTool.fParam[Id].DicPointName.Contains(Value))
                    {
                        MessageBox.Show("ID已存在");
                        dataGridView1.Rows[currentId].Cells[1].Value = "Default";
                        return;
                    }
                    fpTool.fParam[Id].DicPointName[currentId] = dataGridView1.Rows[currentId].Cells[1].Value.ToString();
                }
            }
            catch (Exception)
            {

                throw;
            }
        }

       
        private void comboBox3_SelectedIndexChanged(object sender, EventArgs e)//左右工位
        {
            if (isLoading)
            {
                return;
            }
            if (comboBox3.SelectedItem.ToString() == "Right")
            {
                isRight = true;
                comboBox3.BackColor = Color.LimeGreen;
            }
            else
            {
                isRight = false;
                comboBox3.BackColor = Color.Yellow;
            }
            RoiParam.isInvoke = false;
            LoadToUI();
            ChangeSide();
            RoiParam.isInvoke = true;
            MessageBox.Show("切换成功!");
        }



        #region 检测
        private string detectParamPath = "";
        private string detectRoiPath = "";
        public void InitDgv3()
        {
            detectParamPath = ParamPath.ParamDir + "DetectParam.xml";
            detectRoiPath = ParamPath.ParamDir + "detect.roi";
            DataGridViewTextBoxColumn tb = new DataGridViewTextBoxColumn();
            tb.HeaderText = "名称";
            dataGridView3.Columns.Add(tb);

            DataGridViewTextBoxColumn tb1 = new DataGridViewTextBoxColumn();
            tb1.HeaderText = "当前值";
            dataGridView3.Columns.Add(tb1);

            DataGridViewTextBoxColumn tb2 = new DataGridViewTextBoxColumn();
            tb2.HeaderText = "最小公差";
            dataGridView3.Columns.Add(tb2);

            DataGridViewTextBoxColumn tb3 = new DataGridViewTextBoxColumn();
            tb3.HeaderText = "最大公差";
            dataGridView3.Columns.Add(tb3);


            string[] names = new string[] { "胶宽", "胶高", "面积", "内边距", "外边距" };

            this.dataGridView3.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            this.dataGridView3.ColumnHeadersDefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            this.dataGridView3.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            this.dataGridView3.AllowUserToAddRows = false;
            dataGridView3.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            dataGridView3.RowHeadersVisible = false;
            for (int i = 0; i < names.Length; i++)
            {
                int index = dataGridView3.Rows.Add();
                dataGridView3.Rows[index].Cells[0].Value = names[i];
            }
            
        }

        private void btn_save_detect_Click(object sender, EventArgs e)
        {
            int Id = Convert.ToInt32(SideName.Substring(4, 1)) - 1;
            if (fpTool.detectParam[Id] == null)
            {
                return;
            }
            if (isRight)
            {
                if (MessageBox.Show("是否保存右工位检测参数", "提示", MessageBoxButtons.YesNo) == DialogResult.Yes)
                {
                    StaticOperate.WriteXML(fpTool.detectParam[Id], detectParamPath);
                    hwindow_final3.viewWindow.saveROI(fpTool.DetectRoiList[Id], detectRoiPath);
                }
            }
            else
            {
                if (MessageBox.Show("是否保存左工位检测参数", "提示", MessageBoxButtons.YesNo) == DialogResult.Yes)
                {
                    StaticOperate.WriteXML(fpTool.detectParam[Id], detectParamPath);
                    hwindow_final3.viewWindow.saveROI(fpTool.DetectRoiList[Id], detectRoiPath);
                }
            }
        }

        private void dataGridView3_CellValueChanged(object sender, DataGridViewCellEventArgs e)
        {
            int colIdx = e.ColumnIndex;
            int rowIdx = e.RowIndex;
            if (colIdx != 2 && colIdx != 3)
            {
                return;
            }
            int SideId = Convert.ToInt32(SideName.Substring(4, 1)) - 1;
            int roiID = dataGridView1.CurrentCell == null ? 0 : dataGridView1.CurrentCell.RowIndex;
            if (fpTool.detectParam[SideId] == null)
            {
                fpTool.detectParam[SideId] = new List<DetectParam>();
            }
            if (fpTool.detectParam[SideId].Count <= roiID)
            {
                fpTool.detectParam[SideId].Add(new DetectParam());
            }
            
            try
            {
                if (colIdx == 2)
                {
                    switch (rowIdx)
                    {
                        case 0:
                            fpTool.detectParam[SideId][roiID].MinWidth = double.Parse(dataGridView3.Rows[rowIdx].Cells[colIdx].Value.ToString());
                            break;
                        case 1:
                            fpTool.detectParam[SideId][roiID].MinHeight = double.Parse(dataGridView3.Rows[rowIdx].Cells[colIdx].Value.ToString());
                            break;
                        case 2:
                            fpTool.detectParam[SideId][roiID].MinArea = double.Parse(dataGridView3.Rows[rowIdx].Cells[colIdx].Value.ToString());
                            break;
                        case 3:
                            fpTool.detectParam[SideId][roiID].MinInnerGap = double.Parse(dataGridView3.Rows[rowIdx].Cells[colIdx].Value.ToString());
                            break;
                        case 4:
                            fpTool.detectParam[SideId][roiID].MinOuterGap = double.Parse(dataGridView3.Rows[rowIdx].Cells[colIdx].Value.ToString());
                            break;
                        default:
                            break;
                    }
                }
                else if (colIdx == 3)
                {
                    switch (rowIdx)
                    {
                        case 0:
                            fpTool.detectParam[SideId][roiID].MaxWidth = double.Parse(dataGridView3.Rows[rowIdx].Cells[colIdx].Value.ToString());
                            break;
                        case 1:
                            fpTool.detectParam[SideId][roiID].MaxHeight = double.Parse(dataGridView3.Rows[rowIdx].Cells[colIdx].Value.ToString());
                            break;
                        case 2:
                            fpTool.detectParam[SideId][roiID].MaxArea = double.Parse(dataGridView3.Rows[rowIdx].Cells[colIdx].Value.ToString());
                            break;
                        case 3:
                            fpTool.detectParam[SideId][roiID].MaxInnerGap = double.Parse(dataGridView3.Rows[rowIdx].Cells[colIdx].Value.ToString());
                            break;
                        case 4:
                            fpTool.detectParam[SideId][roiID].MaxOuterGap = double.Parse(dataGridView3.Rows[rowIdx].Cells[colIdx].Value.ToString());
                            break;
                        default:
                            break;
                    }
                }
                dataGridView3.Rows[rowIdx].Cells[colIdx].Style.BackColor = Color.White;
            }
            catch (Exception)
            {
                dataGridView3.Rows[rowIdx].Cells[colIdx].Style.BackColor = Color.Red;
            }
            
        }
     
    
        #endregion


    }

    public class ParamPath
    {
        public static string ParaName = "";
        public static bool IsRight = false;
        public static string ToolType = "";
        public static string ParamDir
        {
            get
            {
                switch (ToolType)
                {
                    case "Calib":
                        if (IsRight)
                        {
                            return AppDomain.CurrentDomain.BaseDirectory + "Calib\\Right\\" + ParaName + "\\";
                        }
                        else
                        {
                            return AppDomain.CurrentDomain.BaseDirectory + "Calib\\Left\\" + ParaName + "\\";
                        }

                    case "GlueGuide":
                        if (IsRight)
                        {
                            return MyGlobal.ConfigPath_Right + ParaName + "\\";
                        }
                        else
                        {
                            return MyGlobal.ConfigPath_Left + ParaName + "\\";
                        }

                }

                return Application.StartupPath + "\\Config" + "\\" + ParaName + "\\";
            }
        }
        public static string Path_Param
        {
            get { return ParamDir + "Fiml.xml"; }
        }
        public static string Path_Setting
        {
            get { return ParamDir + "setting.xml"; }
        }
        public static string Path_roi
        {
            get { return ParamDir + "RoiLineCircle.roi"; }
        }
        public static string Path_tup
        {
            get { return ParamDir + "Calibrate" + ".tup"; }
        }
        public static string Path_CalibPix
        {
            get { return ParamDir + "CalibratePix" + ".txt"; }
        }

        public static void WriteTxt(string fileName, string value)
        {
            try
            {
                //string fileName = AppDomain.CurrentDomain.BaseDirectory + "data.txt";
                using (FileStream fs = new FileStream(fileName, FileMode.OpenOrCreate, FileAccess.Write))
                {
                    using (StreamWriter sw = new StreamWriter(fs))
                    {
                        sw.BaseStream.Seek(0, SeekOrigin.End);
                        sw.WriteLine("{0}\n", value);
                        sw.Flush();
                        sw.Close();
                    }
                }
            }
            catch (Exception)
            {

                throw;
            }
        }

    }
    [Serializable]
    public struct IntersetionCoord
    {
        public double Row;
        public double Col;
        public double Angle;
    }

    //[Serializable]
    //public class FitProfileParam
    //{
    //    /// <summary>
    //    /// 选择最高点左侧
    //    /// </summary>
    //    public bool BeLeft = false;
    //    /// <summary>
    //    /// 有效轮廓起始点
    //    /// </summary>
    //    public int StartPt = 0;
    //    /// <summary>
    //    /// 有效轮廓结束点
    //    /// </summary>
    //    public int EndPt = 0;
    //    /// <summary>
    //    /// 最高点下降距离
    //    /// </summary>
    //    public double UpDownDist = 0;
    //    /// <summary>
    //    /// 单边高度偏移
    //    /// </summary>
    //    public double SigleZoffset = 0;
    //    /// <summary>
    //    /// 最小区间高度
    //    /// </summary>
    //    public double MinZ = 0;
    //    /// <summary>
    //    /// 最大区间高度
    //    /// </summary>
    //    public double MaxZ = 0.5;

    //    public List<RoiParam> roiP = new List<RoiParam>();
    //    public List<string> DicPointName = new List<string>();
    //    /// <summary>
    //    /// 使用定位
    //    /// </summary>
    //    public bool UseFix = false;
    //}

    
    [Serializable]
    public class RoiParam
    {
        public static bool isInvoke = false;
        public static event Action<ValueChangedType,double> ChangeSection;
        #region 锚定轮廓roi 
        [BrowsableAttribute(false)]
        public double AnchorRow { get; set; } = 0;
        [BrowsableAttribute(false)]
        public double AnchorCol { get; set; } = 0;
        #endregion

        /// <summary>
        /// 选择直线段 还是圆弧段 连接段
        /// </summary>
        [BrowsableAttribute(false)]
        public string LineOrCircle { get; set; } = "直线段";

        #region 截面设置
        [Category("\t\t\t\t截面设置")]
        [DisplayName("矩形区域截面数量")]
        [Description("截面数量")]
        public int NumOfSection
        {
            get { return _NumOfSection; }
            set
            {
                if (value==0)
                {
                    value = 10;
                }
                _NumOfSection = value;
                if (isInvoke)
                {
                    ChangeSection?.Invoke(ValueChangedType.数量,_NumOfSection);
                }
            }
        }
        int _NumOfSection = 10;

        [Category("\t\t\t\t截面设置")]
        [DisplayName("矩形长轴")]
        [Description("截面矩形长轴的一半 单位pix")]
        public double Len1
        {
            get { return _Len1; }
            set
            {
                if (value == 0)
                {
                    value = 100;
                }
                _Len1 = value;
                if (isInvoke)
                {
                    ChangeSection?.Invoke(ValueChangedType.长度,_Len1);
                }
                
            }

        }
        double _Len1 = 0;

      [Category("\t\t\t\t截面设置")]
        [DisplayName("矩形短轴")]
        [Description("截面矩形短轴的一半 单位pix")]
        public double Len2
        {
            get { return _Len2; }
            set
            {
                if (value == 0)
                {
                    value = 50;
                }
                _Len2 = value;
                if (isInvoke)
                {
                    ChangeSection?.Invoke(ValueChangedType.宽度,_Len2);
                }
               
            }

        }
        double _Len2 = 0;

        [Category("\t\t\t\t截面设置")]
        [DisplayName("矩形中心行坐标")]
        [Description("矩形中心行坐标 单位pix")]
        public double CenterRow
        {
            get { return _CenterRow; }
            set
            {
                if (value == 0)
                {
                    value = 50;
                }
                _CenterRow = value;
                if (isInvoke)
                {
                    ChangeSection?.Invoke(ValueChangedType.行坐标,_CenterRow);
                }
               
            }

        }
        double _CenterRow = 0;
        [Category("\t\t\t\t截面设置")]
        [DisplayName("矩形中心列坐标")]
        [Description("矩形中心列坐标 单位pix")]
        public double CenterCol
        {
            get { return _CenterCol; }
            set
            {
                if (value == 0)
                {
                    value = 50;
                }
                _CenterCol = value;
                if (isInvoke)
                {
                    ChangeSection?.Invoke(ValueChangedType.列坐标,_CenterCol);
                }
                
            }

        }
        double _CenterCol = 0;
        [BrowsableAttribute(false)]
        [Category("\t\t\t\t截面设置")]
        [DisplayName("矩形角度 phi")]
        [Description("矩形角度 弧度")]
        public double phi
        {
            get
            {
                
                //HTuple rad = new HTuple();
                //HOperatorSet.TupleRad(_phi, out rad);
                return _phi;
            }
            set
            {
                HTuple deg = new HTuple();
                HOperatorSet.TupleDeg(value, out deg);
                phi_deg = deg;
                //if (isInvoke)
                //{
                //    ChangeSection?.Invoke(ValueChangedType.角度);
                //}
               
            }

        }
        [Category("\t\t\t\t截面设置")]
        [DisplayName("矩形角度")]
        [Description("矩形角度 单位度")]
        public double phi_deg
        {
            get
            {              
                return _Deg;
            }
            set
            {
                //HTuple deg = new HTuple();
                //HOperatorSet.TupleDeg(value, out deg);
                HTuple rad = new HTuple();
                HOperatorSet.TupleRad(value, out rad);
                _phi = rad;
                _Deg = value;
                if (isInvoke)
                {
                    ChangeSection?.Invoke(ValueChangedType.角度,_phi);
                }

            }
        }
        double _phi = 0;
        double _Deg = 0;
        #endregion

        #region 拟合直线相对锚定点偏移 取胶槽
        [BrowsableAttribute(false)]
        public double Xoffset2 { get; set; } = 0;
        [BrowsableAttribute(false)]
        public double Yoffset2 { get; set; } = 0;
        public Point StartOffSet1 = new Point(0, 0);//拟合直线相对锚定点偏移
        public Point EndOffSet1  = new Point(0, 0);
        public Point StartOffSet2  = new Point(0, 0);
        public Point EndOffSet2 = new Point(0, 0);
        #endregion

        #region 偏移设置
        [Category("\t\t偏移设置")]
        [DisplayName("X方向偏移")]
        [Description("X方向偏移 单位mm")]
        public double Xoffset
        {
            get
            {
                return _Xoffset;
            }
            set
            {
                _Xoffset = value;
                if (isInvoke)
                {
                    ChangeSection?.Invoke(ValueChangedType.X方向偏移, _Xoffset);
                }
            }
        }
        double _Xoffset = 0;
        [Category("\t\t偏移设置")]
        [DisplayName("Y方向偏移")]
        [Description("Y方向偏移 单位mm")]
        public double Yoffset
        {
            get
            {
                return _Yoffset;
            }
            set
            {
                _Yoffset = value;
                if (isInvoke)
                {
                    ChangeSection?.Invoke(ValueChangedType.Y方向偏移, _Yoffset);
                }
            }
        }
        double _Yoffset = 0;
        [Category("\t\t偏移设置")]
        [DisplayName("Roi方向偏移")]
        [Description("Roi方向偏移 单位mm")]
        public double offset
        {
            get
            {
                return _offset;
            }
            set
            {
                _offset = value;
                if (isInvoke)
                {
                    ChangeSection?.Invoke(ValueChangedType.Roi方向偏移, _offset);
                }
            }
        }
        double _offset = 0;
        [Category("\t\t偏移设置")]
        [DisplayName("Z方向偏移")]
        [Description("Z方向偏移 单位mm")]       
        public double Zoffset
        {
            get
            {
                return _Zoffset;
            }
            set
            {
                _Zoffset = value;
                if (isInvoke)
                {
                    ChangeSection?.Invoke(ValueChangedType.Z方向偏移, _Zoffset);
                }
            }
        }
        double _Zoffset = 0;

        #endregion

        #region 轮廓设置
        [Category("\t\t轮廓设置")]
        [DisplayName("轮廓旋转角度")]
        [Description("轮廓旋转角度设置 单位度")]
        public int AngleOfProfile
        {
            get { return _AngleOfProfile; }
            set
            {
                if (value >360)
                {
                    value = 360;
                }
                if (value<-360)
                {
                    value = -360;
                }
                _AngleOfProfile = value;
                if (isInvoke)
                {                    
                    ChangeSection?.Invoke(ValueChangedType.轮廓旋转角度, _AngleOfProfile);
                }
            }
        }
        int _AngleOfProfile = 0;

        [Category("\t\t轮廓设置")]
        [DisplayName("轮廓平滑")]
        [Description("轮廓平滑系数 3-20 ")]
        public double Sigma
        {
            get { return _Sigma; }
            set
            {
                _Sigma = value;
                if (isInvoke)
                {
                    ChangeSection?.Invoke(ValueChangedType.轮廓平滑, _Sigma);
                }
            }
        }
        double _Sigma = 0.5;

        #endregion

        #region 找线方式
        [TypeConverter(typeof(FileNameConverter))]
        [Category("\t\t\t\t找线方式")]
        [DisplayName("\t找线方式设置")]
        [Description("找线方式设置 极值 或 最高点下降")]
        public string TypeOfFindLine
        {
            get
            {
                return _TypeOfFindLine;
            }
            set
            {
                _TypeOfFindLine = value;
                if (isInvoke)
                {
                    double a = _TypeOfFindLine == "极值" ? 0 : 1;
                    ChangeSection?.Invoke(ValueChangedType.找线方式设置, a);
                }
            }
        }
        string _TypeOfFindLine = "极值";


        /// <summary>
        /// 0极值 1最高点下降
        /// </summary>
        [BrowsableAttribute(false)]
        public int SelectedType
        {
            get
            {
                if (TypeOfFindLine == "极值")
                {
                    return 0;
                }
                else
                {
                    return 1;
                }
            }
            set
            {
            }
        }

        [Category("\t\t\t\t找线方式")]
        [DisplayName("最高点下降距离")]
        [Description("最高点下降距离 单位mm")]
        public double TopDownDist
        {
            get { return _TopDownDist; }
            set
            {
                if (value > 360)
                {
                    value = 360;
                }
                if (value < -360)
                {
                    value = -360;
                }
                _TopDownDist = value;
                if (isInvoke)
                {
                    ChangeSection?.Invoke(ValueChangedType.最高点下降距离, _TopDownDist);
                }
            }
        }
        double _TopDownDist = 0;

        [Category("\t\t\t\t找线方式")]
        [DisplayName("水平方向距离")]
        [Description("离最高点或极值点的距离 单位mm")]
        public double xDist
        {
            get { return _xDist; }
            set
            {
                if (value > 360)
                {
                    value = 360;
                }
                if (value < -360)
                {
                    value = -360;
                }
                _xDist = value;
                if (isInvoke)
                {
                    ChangeSection?.Invoke(ValueChangedType.X方向距离, _xDist);
                }
            }
        }
        double _xDist = 0;


        #endregion

        #region 取点方式设置
        [Category("\t\t\t取点方式设置")]
        [DisplayName("是否取中间点")]
        [Description("对于长边可选择是否 取最外侧边缘和台阶边缘中间点，取中间点为true 否则为false")]
        public bool useMidPt
        {
            get { return _useMidPt; }
            set
            {
                _useMidPt = value;
                if (isInvoke)
                {
                    double a = _useMidPt ? 1 : 0;
                    ChangeSection?.Invoke(ValueChangedType.是否取中间点, a);
                }
            }
        }
        bool _useMidPt = false;

        [Category("\t\t\t取点方式设置")]
        [DisplayName("取轮廓区域中心点")]
        [Description("是否选择区域中心点，取中心点为true 否则为false")]
        public bool useCenter
        {
            get { return _useCenter; }
            set
            {
                _useCenter = value;
                if (isInvoke)
                {
                    double a = _useCenter ? 1 : 0;
                    ChangeSection?.Invoke(ValueChangedType.取轮廓区域中心点, a);
                }
            }
        }
        bool _useCenter = false;


        #endregion

        #region 点筛选

        [Category("\t\t点筛选")]
        [DisplayName("是否取左侧值")]
        [Description("在最高点或极值点的左侧 为 true 否则为 false")]
        public bool useLeft
        {
            get { return _useLeft; }
            set
            {
                _useLeft = value;
                if (isInvoke)
                {
                    double a = _useLeft ? 1 : 0;
                    ChangeSection?.Invoke(ValueChangedType.是否取左侧值, a);
                }
            }
        }
        bool _useLeft = true;

        [Category("\t\t点筛选")]
        [DisplayName("取最近点还是最远点")]
        [Description("是否选择离最高点或极值点的最远距离的点，取最近点为true 否则为false")]
        public bool useNear
        {
            get { return _useNear; }
            set
            {
                _useNear = value;
                if (isInvoke)
                {
                    double a = _useNear ? 1 : 0;
                    ChangeSection?.Invoke(ValueChangedType.取最近点还是最远点, a);
                }
            }
        }
        bool _useNear = false;

        #endregion

        #region Z方向缩放设置

        [Category("\tZ方向缩放设置")]
        [DisplayName("是否启用Z向缩放")]
        [Description("是否选择区域中心点，启用缩放为true 否则为false")]
        public bool useZzoom
        {
            get { return _useZzoom; }
            set
            {
                _useZzoom = value;
                if (isInvoke)
                {
                    double a = _useZzoom ? 1 : 0;
                    ChangeSection?.Invoke(ValueChangedType.是否启用Z向缩放, a);
                }
            }
        }
        bool _useZzoom = false;

        [Category("\tZ方向缩放设置")]
        [DisplayName("轮廓Z向拉伸")]
        [Description("轮廓沿Z向拉伸比列 单位为倍数")]
        public double ClippingPer
        {
            get { return _ClippingPer; }
            set
            {
                _ClippingPer = value;
                if (isInvoke)
                {
                    ChangeSection?.Invoke(ValueChangedType.轮廓Z向拉伸, _ClippingPer);
                }
            }
        }
        double _ClippingPer = 0;


        #endregion

        #region 高度滤波设置
        [Category("高度滤波设置")]
        [DisplayName("高度方向滤波最大百分比")]
        [Description("高度方向滤波最大百分比 范围设置0-100")]

        public double ZftMax
        {
            get
            { return _ZftMax; }
            set
            {
                if (value > 100)
                {
                    value = 100;
                }
                if (value < 0)
                {
                    value = 0;
                }
                _ZftMax = value;
                if (isInvoke)
                {
                    ChangeSection?.Invoke(ValueChangedType.高度方向滤波最大百分比, _ZftMax);
                }
            }
        }
        double _ZftMax = 0;

        [Category("高度滤波设置")]
        [DisplayName("高度方向滤波最小百分比")]
        [Description("高度方向滤波最小百分比 范围设置0-100")]
        public double ZftMin
        {
            get
            { return _ZftMin; }
            set
            {
                if (value > 100)
                {
                    value = 100;
                }
                if (value < 0)
                {
                    value = 0;
                }
                _ZftMin = value;
                if (isInvoke)
                {
                    ChangeSection?.Invoke(ValueChangedType.高度方向滤波最小百分比, _ZftMin);
                }
            }
        }
        double _ZftMin = 0;
        [Category("高度滤波设置")]
        [DisplayName("高度方向滤波半径")]
        [Description("高度方向滤波半径 单位mm")]
        public double ZftRad
        {
            get { return _ZftRad; }
            set
            {
                _ZftRad = value;
                if (isInvoke)
                {
                    ChangeSection?.Invoke(ValueChangedType.高度方向滤波半径, _ZftRad);
                }
            }
        }
        double _ZftRad = 0;

        #endregion

        #region 直线拟合设置
        [Category("\t直线拟合设置")]
        [DisplayName("直线滤波系数")]
        [Description("直线找点滤波系数 范围设置0-0.5")]
        public double SmoothCont
        {
            get { return _SmoothCont; }

            set
            {
                if (value < 0)
                {
                    value = 0;
                }
                if (value > 0.5)
                {
                    value = 0.5;
                }
                _SmoothCont = value;
                if (isInvoke)
                {
                    ChangeSection?.Invoke(ValueChangedType.直线滤波系数, _SmoothCont);
                }
            }
        }
        double _SmoothCont = 0;

        #endregion


        public RoiParam Clone()
        {
            RoiParam temp = new RoiParam();
            temp = (RoiParam)this.MemberwiseClone();
            return temp;
        }
        public class FileNameConverter : StringConverter
        {
            public override bool GetStandardValuesSupported(ITypeDescriptorContext context)
            {
                return true;
            }
            public override StandardValuesCollection GetStandardValues(ITypeDescriptorContext context)
            {
                return new StandardValuesCollection(new string[] {"极值","最高点下降"});
            }
        }
    }

    public enum ValueChangedType
    {
        数量 = 1,
        长度 = 2,
        宽度 = 3,
        角度 = 4,
        行坐标 = 5,
        列坐标 = 6,
        X方向偏移 =7,
        Y方向偏移 =8,
        Roi方向偏移 =9,
        Z方向偏移 = 10,
        轮廓旋转角度 = 11,
        最高点下降距离 =12,
        X方向距离 =13,
        是否取左侧值 =14,
        是否取中间点 =15,
        取最近点还是最远点 =16,
        取轮廓区域中心点 =17,
        是否启用Z向缩放 =18,
        轮廓Z向拉伸 =19,
        轮廓平滑 =20,
        直线滤波系数 =21,
        高度方向滤波最大百分比 =22,
        高度方向滤波最小百分比 =23,
        高度方向滤波半径 =24,
        找线方式设置 = 25
    }

    [Serializable]
    public class FitProfileParam
    {
        #region 截面参数设置
       
        #endregion
        /// <summary>
        /// 选择最高点左侧
        /// </summary>
        public bool BeLeft = false;
        /// <summary>
        /// 有效轮廓起始点
        /// </summary>
        public int StartPt = 0;
        /// <summary>
        /// 有效轮廓结束点
        /// </summary>
        public int EndPt = 0;
        /// <summary>
        /// 最高点下降距离
        /// </summary>
        public double UpDownDist = 0;
        /// <summary>
        /// 单边高度偏移
        /// </summary>
        public double SigleZoffset = 0;
        /// <summary>
        /// 最小区间高度
        /// </summary>
        public double MinZ = 0;
        /// <summary>
        /// 最大区间高度
        /// </summary>
        public double MaxZ = 0.5;

        public List<RoiParam> roiP = new List<RoiParam>();
        public List<string> DicPointName = new List<string>();//点位名称
        /// <summary>
        /// 使用定位
        /// </summary>
        public bool UseFix = false;
    }

    public class DetectParam
    {
        public double MinWidth;
        public double MaxWidth;
        public double MinHeight;
        public double MaxHeight;
        public double MinArea;
        public double MaxArea;
        public double MinInnerGap;
        public double MaxInnerGap;
        public double MinOuterGap;
        public double MaxOuterGap;
    }

}