﻿using Microsoft.VisualStudio.TestTools.UnitTesting;
using FlowRunner.Engine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Reflection;

using frEnvironment = FlowRunner.Engine.ChainEnvironment;
using FloorDataFrame = FlowRunner.Engine.ChainEnvironment.FloorDataFrame;
using FlowRunner.Utl;

namespace FlowRunner.Engine.Tests
{
    [TestClass()]
    public class ChainEnvironmentTests
    {

        #region(GetValue)
        //同階層に定義のある変数の値を取得しようとしたとき
        [TestMethod()]
        public void GetValueTest() {
            frEnvironment environment = new frEnvironment();

            FloorDataFrame currentFloor = environment.GetField<FloorDataFrame>("currentFloor");
            string variableName = "t";
            string value = "t-value";

            currentFloor.Variables.Add(variableName, value);

            Assert.AreEqual(value, environment.GetValue(variableName));

        }

        //上位階層に定義のある変数の値を取得しようとしたとき
        [TestMethod()]
        public void GetValueTest1() {
            frEnvironment environment = new frEnvironment();

            List<FloorDataFrame> floorDataFrames = environment.GetField<List<FloorDataFrame>>("floorDataFrames");
            var floorAbove = floorDataFrames[0];
            var currentFloor = new FloorDataFrame();
            floorDataFrames.Add(currentFloor);
            environment.SetField("currentFloorNo", 1);
            environment.SetField("currentFloor", currentFloor);

            string variableName = "t";
            string value = "t-value";

            floorAbove.Variables.Add(variableName, value);

            Assert.AreEqual(value, environment.GetValue(variableName));
        }

        //大域環境にて未定義の変数にアクセスしようとした
        [TestMethod()]
        public void GetValueTest2() {
            frEnvironment environment = new frEnvironment();

            string variableName = "t";

            Assert.ThrowsException<frEnvironment.UndefinedVariableException>(() => {
                environment.GetValue(variableName);
            });
        }

        //現階層が大域環境でなく、また上位階層に該当する変数の定義がない場合に値を取得しょうとしたとき
        [TestMethod()]
        public void GetValueTest3() {
            frEnvironment environment = new frEnvironment();

            List<FloorDataFrame> floorDataFrames = environment.GetField<List<FloorDataFrame>>("floorDataFrames");
            var currentFloor = new FloorDataFrame();
            floorDataFrames.Add(currentFloor);
            environment.SetField("currentFloorNo", 1);
            environment.SetField("currentFloor", currentFloor);

            string variableName = "t";

            Assert.ThrowsException<frEnvironment.UndefinedVariableException>(() => {
                environment.GetValue(variableName);
            });
        }
        #endregion

        #region(SetValue)
        //現階層が大域環境で、すでに変数の定義がある場合
        [TestMethod()]
        public void SetValueTest() {
            frEnvironment environment = new frEnvironment();

            FloorDataFrame currentFloor = environment.GetField<FloorDataFrame>("currentFloor");
            string variableName = "t";
            string oldValue = "old-value";
            string newValue = "new-value";

            currentFloor.Variables.Add(variableName, oldValue);

            environment.SetValue(variableName, newValue);

            Assert.AreEqual(newValue, currentFloor.Variables[variableName]);
        }

        //現階層が非大域環境で、現階層にすでに変数の定義がある場合
        [TestMethod()]
        public void SetValueTest1() {
            frEnvironment environment = new frEnvironment();

            List<FloorDataFrame> floorDataFrames = environment.GetField<List<FloorDataFrame>>("floorDataFrames");
            var currentFloor = new FloorDataFrame();
            floorDataFrames.Add(currentFloor);
            environment.SetField("currentFloorNo", 1);
            environment.SetField("currentFloor", currentFloor);

            string variableName = "t";
            string oldValue = "old-value";
            string newValue = "new-value";

            currentFloor.Variables.Add(variableName, oldValue);

            environment.SetValue(variableName, newValue);

            Assert.AreEqual(newValue, currentFloor.Variables[variableName]);
        }
        //現階層が大域環境で、変数の定義がない場合
        [TestMethod()]
        public void SetValueTest2() {
            frEnvironment environment = new frEnvironment();

            FloorDataFrame currentFloor = environment.GetField<FloorDataFrame>("currentFloor");
            string variableName = "t";
            string value = "t-value";

            environment.SetValue(variableName, value);

            Assert.AreEqual(value, currentFloor.Variables[variableName]);
        }
        //現階層が非大域環境で、現階層から大域環境まで含めて変数の定義がない場合
        [TestMethod()]
        public void SetValueTest3() {
            frEnvironment environment = new frEnvironment();

            List<FloorDataFrame> floorDataFrames = environment.GetField<List<FloorDataFrame>>("floorDataFrames");
            var floorAbove = floorDataFrames[0];
            var currentFloor = new FloorDataFrame();
            floorDataFrames.Add(currentFloor);
            environment.SetField("currentFloorNo", 1);
            environment.SetField("currentFloor", currentFloor);

            string variableName = "t";
            string value = "t-value";

            environment.SetValue(variableName, value);

            Assert.AreEqual(value, currentFloor.Variables[variableName]);
            Assert.AreEqual(false, floorAbove.Variables.ContainsKey(variableName));
        }
        //大域環境に定義あり、第一層に定義あり、第二層が現階層で定義なしの場合
        [TestMethod()]
        public void SetValueTest4() {
            frEnvironment environment = new frEnvironment();

            List<FloorDataFrame> floorDataFrames = environment.GetField<List<FloorDataFrame>>("floorDataFrames");
            floorDataFrames.Add(new FloorDataFrame());
            floorDataFrames.Add(new FloorDataFrame());
            var globalfloor = floorDataFrames[0];
            var floorAbove = floorDataFrames[1];
            var currentFloor = floorDataFrames[2];
            environment.SetField("currentFloorNo", 2);
            environment.SetField("currentFloor", currentFloor);

            string variableName = "t";
            string oldValue_0 = "old-value_0";
            string oldValue_1 = "old-value_1";
            string newValue = "new-value";

            globalfloor.Variables.Add(variableName, oldValue_0);
            floorAbove.Variables.Add(variableName, oldValue_1);

            environment.SetValue(variableName, newValue);

            Assert.AreEqual(oldValue_0, globalfloor.Variables[variableName]);
            Assert.AreEqual(newValue, floorAbove.Variables[variableName]);
            Assert.AreEqual(false, currentFloor.Variables.ContainsKey(variableName));
        }




        #endregion

        #region(Down)
        [TestMethod()]
        public void DownTest() {
            frEnvironment environment = new frEnvironment();

            FloorDataFrame oldCurrentFloor = environment.GetField<FloorDataFrame>("currentFloor");
            int oldCurrentFloorNo = environment.GetField<int>("currentFloorNo");
            List<string> returnValues = new List<string> { "rv0", "rv1" };
            List<string> arguments = new List<string> { "arg0", "arg1" };

            environment.Down(returnValues, arguments);

            FloorDataFrame newCurrentFloor = environment.GetField<FloorDataFrame>("currentFloor");
            int newCurrentFloorNo = environment.GetField<int>("currentFloorNo");

            Assert.AreEqual(false, oldCurrentFloor == newCurrentFloor);
            Assert.AreEqual(oldCurrentFloorNo + 1, newCurrentFloorNo);
            Assert.AreEqual(returnValues, newCurrentFloor.ReturnValues);
            Assert.AreEqual(arguments, newCurrentFloor.Arguments);
        }
        #endregion

        #region(PullArguments)
        [TestMethod()]
        public void PullArgumentsTest() {
            frEnvironment environment = new frEnvironment();

            FloorDataFrame oldCurrentFloor = environment.GetField<FloorDataFrame>("currentFloor");
            int oldCurrentFloorNo = environment.GetField<int>("currentFloorNo");
            string callerArgument0 = "call-arg0";
            string callerArgument1 = "call-arg1";
            string pullArgument0 = "arg0";
            string pullArgument1 = "arg1";
            string value2_arg0 = "value2";
            string value3_arg1 = "value3";
            List<string> returnValues = new List<string> { "rv0", "rv1" };
            List<string> callerArguments = new List<string> { callerArgument0, callerArgument1 };
            List<string> pullArguments = new List<string> { pullArgument0, pullArgument1 };

            environment.SetValue(callerArgument0, value2_arg0);
            environment.SetValue(callerArgument1, value3_arg1);
            environment.Down(returnValues, callerArguments);

            environment.PullArguments(pullArguments);

            FloorDataFrame newCurrentFloor = environment.GetField<FloorDataFrame>("currentFloor");
            int newCurrentFloorNo = environment.GetField<int>("currentFloorNo");

            Assert.AreEqual(true, newCurrentFloor.Variables.ContainsKey(pullArgument0));
            Assert.AreEqual(true, newCurrentFloor.Variables.ContainsKey(pullArgument1));
            Assert.AreEqual(value2_arg0, environment.GetValue(pullArgument0));
            Assert.AreEqual(value3_arg1, environment.GetValue(pullArgument1));
        }
        #endregion

        #region(Up)
        [TestMethod()]
        public void UpTest() {
            frEnvironment environment = new frEnvironment();

            FloorDataFrame oldCurrentFloor = environment.GetField<FloorDataFrame>("currentFloor");
            int oldCurrentFloorNo = environment.GetField<int>("currentFloorNo");
            string callerReturnValue0 = "caller-rv0";
            string callerReturnValue1 = "caller-rv1";
            string returnReturnValue0 = "return-rv0";
            string returnReturnValue1 = "return-rv1";
            string value0_Caller_Old = "value0";
            string value1_Caller_Old = "value1";
            string value2_Return_New = "value2";
            string value3_Return_New = "value3";
            List<string> callerReturnValues = new List<string> { callerReturnValue0, callerReturnValue1 };
            List<string> returnReturnValues = new List<string> { returnReturnValue0, returnReturnValue1 };
            List<string> arguments = new List<string> { "arg0", "arg1" };

            environment.SetValue(callerReturnValue0, value0_Caller_Old);
            environment.SetValue(callerReturnValue1, value1_Caller_Old);
            environment.Down(callerReturnValues, arguments);

            environment.SetValue(returnReturnValue0, value2_Return_New);
            environment.SetValue(returnReturnValue1, value3_Return_New);

            environment.Up(returnReturnValues);


            Assert.AreEqual(value2_Return_New, environment.GetValue(callerReturnValue0));
            Assert.AreEqual(value3_Return_New, environment.GetValue(callerReturnValue1));
        }
        #endregion

        #region()
        #endregion

    }



}