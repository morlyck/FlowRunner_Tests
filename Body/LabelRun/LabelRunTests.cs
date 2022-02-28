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

        #region(ShotRunTest ビルドインコマンド)
        //nop命令
        //PackCodeの指定の省略形式
        [TestMethod()]
        public void ShotRunTest() {
            RunningContext context = new RunningContext();
            LabelRun labelRun = new LabelRun();
            LabelRunOrdertaker ordertaker = new LabelRunOrdertaker();
            labelRun.LabelRunOrdertaker = ordertaker;
            ordertaker.catchException_LabelResolutionMiss = (context, packCode) => true;
            string packCode = "t";
            int expected1 = 1;
            int expected2 = 2;

            context.Statements = new Statement[] {
                new StatementDummy("nop"),
                new StatementDummy("nop"),
                new StatementDummy("nop")
            };

            context.CurrentPackCode = packCode;
            context.IsHalting = false;

            labelRun.ShotRun(context);
            Assert.AreEqual(expected1, context.ProgramCounter);

            labelRun.ShotRun(context);
            Assert.AreEqual(expected2, context.ProgramCounter);
        }
        //halt命令
        //PackCodeの指定の省略形式
        [TestMethod()]
        public void ShotRunTest1() {
            RunningContext context = new RunningContext();
            LabelRun labelRun = new LabelRun();
            LabelRunOrdertaker ordertaker = new LabelRunOrdertaker();
            labelRun.LabelRunOrdertaker = ordertaker;
            ordertaker.catchException_LabelResolutionMiss = (context, packCode) => true;
            string packCode = "t";
            int expected1 = 1;
            int expected2 = 1;

            context.Statements = new Statement[] {
                new StatementDummy("halt"),
                new StatementDummy("nop"),
                new StatementDummy("nop")
            };

            context.CurrentPackCode = packCode;
            context.IsHalting = false;

            labelRun.ShotRun(context);
            Assert.AreEqual(expected1, context.ProgramCounter);

            labelRun.ShotRun(context);
            Assert.AreEqual(expected2, context.ProgramCounter);
        }
        //jump命令
        //PackCodeの指定の省略形式
        [TestMethod()]
        public void ShotRunTest2() {
            RunningContext context = new RunningContext();
            LabelRun labelRun = new LabelRun();
            LabelRunOrdertaker ordertaker = new LabelRunOrdertaker();
            labelRun.LabelRunOrdertaker = ordertaker;
            ordertaker.catchException_LabelResolutionMiss = (context, packCode) => true;
            string label = "jump_point";
            int label_PCValue = 2;
            string packCode = "t";
            int expected = 2;

            context.Labels.Add(label, label_PCValue);
            context.Statements = new Statement[] {
                new StatementDummy("jump", "", label),
                new StatementDummy("nop"),
                new StatementDummy("nop")
            };

            context.CurrentPackCode = packCode;
            context.IsHalting = false;

            labelRun.ShotRun(context);

            Assert.AreEqual(expected, context.ProgramCounter);
        }
        //call命令
        //PackCodeの指定の省略形式
        [TestMethod()]
        public void ShotRunTest3() {
            RunningContext context = new RunningContext();
            LabelRun labelRun = new LabelRun();
            LabelRunOrdertaker ordertaker = new LabelRunOrdertaker();
            labelRun.LabelRunOrdertaker = ordertaker;
            ordertaker.catchException_LabelResolutionMiss = (context, packCode) => true;
            string label = "call_point";
            int label_PCValue = 2;
            string packCode = "t";
            int expected = 2;
            int expected_Stack = 0;

            context.Labels.Add(label, label_PCValue);
            context.Statements = new Statement[] {
                new StatementDummy("call", "", label),
                new StatementDummy("nop"),
                new StatementDummy("nop")
            };

            context.CurrentPackCode = packCode;
            context.IsHalting = false;

            labelRun.ShotRun(context);

            Assert.AreEqual(expected, context.ProgramCounter);
            Assert.AreEqual(expected_Stack, context.CallStack.Peek().ProgramCounter);
        }
        //return命令
        [TestMethod()]
        public void ShotRunTest4() {
            RunningContext context = new RunningContext();
            LabelRun labelRun = new LabelRun();
            LabelRunOrdertaker ordertaker = new LabelRunOrdertaker();
            labelRun.LabelRunOrdertaker = ordertaker;
            ordertaker.catchException_LabelResolutionMiss = (context, packCode) => true;
            string label = "return_point";
            int label_PCValue = 2;
            string packCode = "t";
            int expected = 1;

            context.Labels.Add(label, label_PCValue);
            context.Statements = new Statement[] {
                new StatementDummy("call", "", label),
                new StatementDummy("nop"),
                new StatementDummy("return")
            };

            context.CurrentPackCode = packCode;
            context.IsHalting = false;

            //call
            labelRun.ShotRun(context);
            //return
            labelRun.ShotRun(context);

            Assert.AreEqual(expected, context.ProgramCounter);
        }
        //int命令
        [TestMethod()]
        public void ShotRunTest5() {
            RunningContext context = new RunningContext();
            LabelRun labelRun = new LabelRun();
            LabelRunOrdertaker ordertaker = new LabelRunOrdertaker();
            labelRun.LabelRunOrdertaker = ordertaker;
            ordertaker.catchException_LabelResolutionMiss = (context, packCode) => true;
            string label = "int_point";
            int label_PCValue = 2;
            string packCode = "t";
            int expected = 1;
            int expected_int = 3;
            int expected_intReturn = 1;

            context.Labels.Add(label, label_PCValue);
            context.Statements = new Statement[] {
                new StatementDummy("int", "", label),
                new StatementDummy("nop"),
                new StatementDummy("nop"),//2
                new StatementDummy("nop"),
                new StatementDummy("return")
            };

            context.CurrentPackCode = packCode;
            context.IsHalting = false;

            //割り込み前
            labelRun.ShotRun(context);

            Assert.AreEqual(expected, context.ProgramCounter);

            //割り込み中
            labelRun.ShotRun(context);

            Assert.AreEqual(expected_int, context.ProgramCounter);

            //nop
            labelRun.ShotRun(context);

            //return
            labelRun.ShotRun(context);

            Assert.AreEqual(expected_intReturn, context.ProgramCounter);
        }
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

        public Func<IRunningContext, string, CommandExecutionContext, bool>? executionExpansionCommand = null;
        public bool ExecutionExpansionCommand(IRunningContext runningContext, string commandSymbol, CommandExecutionContext commandExecutionContext) {
            if (executionExpansionCommand == null) return false;
            return executionExpansionCommand(runningContext, commandSymbol, commandExecutionContext);
        }

        public bool CatchException_InvalidCommand(IRunningContext runningContext, InvalidCommandException e) {
            throw new NotImplementedException();
        }

        public Func<IRunningContext, LabelResolutionMissException, bool> catchException_LabelResolutionMiss = null;
        public bool CatchException_LabelResolutionMiss(IRunningContext runningContext, LabelResolutionMissException e) {
            if (catchException_LabelResolutionMiss == null) return false;
            return catchException_LabelResolutionMiss(runningContext, e);
        }

        public bool CatchException_ProgramCounterOutOfRange(IRunningContext runningContext, ProgramCounterOutOfRangeException e) {
            throw new NotImplementedException();
        }

        public bool CatchException_CallStackEmptyPop(IRunningContext runningContext, CallStackEmptyPopException e) {
            throw new NotImplementedException();
        }
    }

    public class StatementDummy : Statement
    {
        public StatementDummy(string CommandSymbol, string PackCode = "", string Label = "", bool ArgumentEvaluationExpansionMode = false, string ArgumentText = "") {
            this.CommandSymbol = CommandSymbol;
            this.PackCode = PackCode;
            this.Label = Label;
            this.ArgumentEvaluationExpansionMode = ArgumentEvaluationExpansionMode;
            this.ArgumentText = ArgumentText;
        }

    }

}