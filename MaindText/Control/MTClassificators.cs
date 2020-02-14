using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MaindText.Model;

namespace MaindText.Control
{
    public class MTClassificator
    {
        #region Parameters

        private static double _iTopInterval = 20;
        private static double _iMidInterval = 1;

        private static double _iIncreaseDay = 2;
        private static double _iDecreaseDayNormal = 1;
        private static double _iDecreaseDayLow = 0.5;

        private static double _dTopScore = 9;
        private static double _dMidScore = 3;
        private static double _dLowScore = 1;

        private static double _dCeilingScore = 20;

        #endregion

        #region Initialization

        static public void Initialize()
        {
        }

        #endregion

        #region Methodes

        static public void UpdateAllPatches()
        {
            //Patches
            foreach (MTPatchModel objPatch in MTControler.TextManager.LstPatch)
            {
                DoUpdatePatch(objPatch);                
            }
            UpdateAllPositions();            
        }

        static public void UpdateListPatches(List<MTPatchModel> lstPatches)
        {
            //Patches
            foreach (MTPatchModel objPatch in lstPatches)
            {
                DoUpdatePatch(objPatch);                
            }
            UpdateAllPositions();
        }

        static public void UpdatePatch(MTPatchModel objPatch)
        {
            DoUpdatePatch(objPatch);
            UpdateAllPositions();
        }

        private static void DoUpdatePatch(MTPatchModel objPatch)
        {
            double dScore = 0;
            DateTime dtLastTest = objPatch.Created;
            double dDecreaseDay = _iDecreaseDayNormal;
            //Testes
            foreach (MTStructTest objTest in objPatch.lstTest)
            {
                double dPartialScore = GetPoints(dtLastTest, objTest.Date, objTest.WasCorrect == true);
                dDecreaseDay = GetDecreaseDay(objPatch.lstTest.GetRange(0, objPatch.lstTest.IndexOf(objTest)));
                dScore -= GetDecreaseByTime(objTest.Date.Subtract(dtLastTest), dDecreaseDay);
                //Low Limit
                dScore = dScore < 0 ? 0 : dScore;
                dScore += dPartialScore;
                //Top Limit
                dScore = dScore > 100 ? 100 : dScore;
                dtLastTest = objTest.Date;
            }
            //Top Limit
            dScore = dScore > 100 ? 100 : dScore;
            //Decrease by Time
            dScore -= GetDecreaseByTime(DateTime.Now.Subtract(dtLastTest), dDecreaseDay);
            objPatch.IsMemorized = dDecreaseDay == _iDecreaseDayLow; 
            //Low Limit
            dScore = dScore < 0 ? 0 : dScore;

            //Final Score
            objPatch.Score = dScore;
        }

        public static double GetPoints(DateTime dtLastTest, DateTime dtNextTest, bool bWasCorrect)
        {
            double dPartialScore = 0;
            TimeSpan dtInterval = dtNextTest.Subtract(dtLastTest);
            if (bWasCorrect)
            {
                //Top Score
                if (dtInterval.TotalHours >= _iTopInterval)
                {
                    dPartialScore += _dTopScore;
                    dPartialScore += (dtInterval.Days * _iIncreaseDay);
                }
                //Middle Score
                else if (dtInterval.TotalHours >= _iMidInterval)
                    dPartialScore += _dMidScore;
                //Low Score
                else if (dtInterval.TotalHours < _iMidInterval)
                    dPartialScore += _dLowScore;
            }
            //Ceiling Limit
            dPartialScore = dPartialScore > _dCeilingScore ? _dCeilingScore : dPartialScore;
            return dPartialScore;
        }

        public static double GetDecreaseByTime(TimeSpan dtInterval, double dDecreaseDay)
        {
            double dDecreaseByDay = dtInterval.Days;
            //Factor
            dDecreaseByDay = Convert.ToInt16(dDecreaseByDay * dDecreaseDay);
            dDecreaseByDay = dDecreaseByDay > 0 ? dDecreaseByDay - 1 : 0;
            return dDecreaseByDay;
        }

        static public double GetDecreaseDay(List<MTStructTest> lstTest)
        {
            lstTest.Reverse();
            List<MTStructTest> lstCorrectTests = new List<MTStructTest>();
            //Count
            foreach (MTStructTest objTest in lstTest)
            {
                if (!objTest.WasCorrect)
                    break;
                lstCorrectTests.Add(objTest);
            }
            if (lstCorrectTests.Count < 8)
                return _iDecreaseDayNormal;
            //Points
            DateTime dtFirst = lstCorrectTests.First().Date;
            lstCorrectTests.Remove(lstCorrectTests.First());
            double dTotalScore = 0;
            foreach (MTStructTest objTest in lstCorrectTests)
            {
                dTotalScore += GetPoints(objTest.Date, dtFirst, true);
                dtFirst = objTest.Date;
            }
            if (dTotalScore > 150)
                return _iDecreaseDayLow;
            else
                return _iDecreaseDayNormal;
        }

        static public void UpdateAllPositions()
        {
            //Patches
            foreach (MTPatchModel objPatch in MTControler.TextManager.LstPatch)
            {
                DoUpdatePosition(objPatch);                
            }
            MTControler.TextManager.FirePropertyChanged("DgCollection");
        }

        private static void DoUpdatePosition(MTPatchModel objPatch)
        {
            TimeSpan dtInterval = DateTime.Now.Subtract(objPatch.LastDateTest);
            double dPrePosition = 0;
            if(objPatch.IsMemorized)
                dPrePosition = ((dtInterval.TotalSeconds / 2) / objPatch.Score)/100;
            else
                dPrePosition = (dtInterval.TotalSeconds / objPatch.Score) / 100;
            //Top Score
            if (dtInterval.TotalHours >= _iTopInterval)
                objPatch.PositionBrain = dPrePosition * 1.2;
            //Middle Score
            else if (dtInterval.TotalHours >= _iMidInterval)
                objPatch.PositionBrain = dPrePosition;
            //Low Score
            else if (dtInterval.TotalHours < _iMidInterval)
                objPatch.PositionBrain = dPrePosition * 0.5;
        }

        #endregion
    }
}
