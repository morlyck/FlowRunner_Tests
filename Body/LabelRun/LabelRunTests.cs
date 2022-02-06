using Microsoft.VisualStudio.TestTools.UnitTesting;
using FlowRunner.Engine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FlowRunner.Engine.Tests
{
    [TestClass()]
    public class LabelRunTests
    {
        #region(GetStatementIndex)
        //・カレントとパックコードが違ったとき
        //・パックを更新せずにインデックスを取得する
        //・Labelsがnull のときに例外を投げること
        //・Labelsに指定されたラベルがないときに例外を投げる

        //カレントのパックコードと指定されたパックコードが異なったときに
        //新たにパックを取得してからインデックスを求める場合
        [TestMethod()]
        public void GetStatementIndex_LabelResolutionTest() {
            RunningContext context = new RunningContext();
            LabelRunOrdertaker ordertaker = new LabelRunOrdertaker();
            ordertaker.getPack = (context, packCode) => {
                Pack pack = new Pack();
                switch (packCode) {
                    case "t":
                        pack.Labels.Add("a", 1);
                        pack.Labels.Add("b", 10);
                        pack.Labels.Add("c", 100);
                        return pack;
                }
                return null;
            };
            LabelRun labelRun = new LabelRun();
            labelRun.LabelRunOrdertaker = ordertaker;
            string currentPackCode = "current";
            string packCode = "t";
            string label = "b";
            int expected = 10;

            context.CurrentPackCode = currentPackCode;
            int index = labelRun.GetStatementIndex_LabelResolution(context, packCode, label);

            Assert.AreEqual(index, expected);
        }

        //カレントのパックコードを指定して、インデックスを取得する
        //パックは新たに取得しない
        [TestMethod()]
        public void GetStatementIndex_LabelResolutionTest1() {
            RunningContext context = new RunningContext();
            context.Labels.Add("a", 1);
            context.Labels.Add("b", 10);
            context.Labels.Add("c", 100);
            LabelRun labelRun = new LabelRun();
            string currentPackCode = "current";
            string packCode = "current";
            string label = "b";
            int expected = 10;

            context.CurrentPackCode = currentPackCode;
            int index = labelRun.GetStatementIndex_LabelResolution(context, packCode, label);

            Assert.AreEqual(index, expected);
        }

        //新たに取得したパックがnullだったときに例外を投げる
        [TestMethod()]
        public void GetStatementIndex_LabelResolutionTest2() {
            RunningContext context = new RunningContext();
            LabelRunOrdertaker ordertaker = new LabelRunOrdertaker();
            ordertaker.getPack = (context, packCode) => {
                Pack pack = new Pack();
                pack.Labels = null;
                return pack;
            };
            LabelRun labelRun = new LabelRun();
            labelRun.LabelRunOrdertaker = ordertaker;
            string currentPackCode = "current";
            string packCode = "t";
            string label = "b";

            context.CurrentPackCode = currentPackCode;

            Assert.ThrowsException<LabelResolutionMissException>(() => {
                labelRun.GetStatementIndex_LabelResolution(context, packCode, label);
            });
        }

        //該当するラベルがない場合は例外を投げる
        [TestMethod()]
        public void GetStatementIndex_LabelResolutionTest3() {
            RunningContext context = new RunningContext();
            context.Labels.Add("a", 1);
            context.Labels.Add("b", 10);
            context.Labels.Add("c", 100);
            LabelRun labelRun = new LabelRun();
            string currentPackCode = "current";
            string packCode = "current";
            string label = "non";

            context.CurrentPackCode = currentPackCode;

            Assert.ThrowsException<LabelResolutionMissException>(() => {
                labelRun.GetStatementIndex_LabelResolution(context, packCode, label);
            });
        }
        #endregion

        #region()
        #endregion

        #region()
        #endregion

    }

    public class LabelRunOrdertaker : ILabelRunOrdertaker
    {
        public Func<IRunningContext, string, Pack> getPack = null;
        public Pack GetPack(IRunningContext runningContext, string packCode) {
            return getPack(runningContext, packCode);
        }
        public CommandExecutionContext Evaluation_ArgumentExpansion(IRunningContext runningContext, string commandSymbol, string packCode, string label, string expansionArgumentText) {
            throw new NotImplementedException();
        }

        public bool ExecutionExpansionCommand(IRunningContext runningContext, string commandSymbol, CommandExecutionContext commandExecutionContext) {
            throw new NotImplementedException();
        }

        public bool CatchException_InvalidCommand(IRunningContext runningContext, InvalidCommandException e) {
            throw new NotImplementedException();
        }

        public bool CatchException_LabelResolutionMiss(IRunningContext runningContext, LabelResolutionMissException e) {
            throw new NotImplementedException();
        }

        public bool CatchException_ProgramCounterOutOfRange(IRunningContext runningContext, ProgramCounterOutOfRangeException e) {
            throw new NotImplementedException();
        }

    }

}