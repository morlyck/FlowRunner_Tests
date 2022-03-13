using Microsoft.VisualStudio.TestTools.UnitTesting;
using FlowRunner.Engine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Reflection;

using FlowRunner.Engine;

using frEnvironment = FlowRunner.Engine.ChainEnvironment;
//using FloorDataFrame<DataType> = FlowRunner.Engine.ChainEnvironmentDataHolder<DataType>.FloorDataFrame<DataType>;
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

            IChainEnvironmentDataHolder dataHolder = environment.GetDataHolder(typeof(string).AssemblyQualifiedName);;

            FloorDataFrame<string> currentFloor = dataHolder.GetField<FloorDataFrame<string>>("currentFloor");
            string variableName = "t";
            string value = "t-value";

            currentFloor.Variables.Add(variableName, value);

            Assert.AreEqual(value, environment.GetValue(variableName));

        }

        //上位階層に定義のある変数の値を取得しようとしたとき
        [TestMethod()]
        public void GetValueTest1() {
            frEnvironment environment = new frEnvironment();
            IChainEnvironmentDataHolder dataHolder = environment.GetDataHolder(typeof(string).AssemblyQualifiedName);;

            List<FloorDataFrame<string>> floorDataFrames = dataHolder.GetField<List<FloorDataFrame<string>>>("floorDataFrames");
            var floorAbove = floorDataFrames[0];
            var currentFloor = new FloorDataFrame<string>();
            floorDataFrames.Add(currentFloor);
            dataHolder.SetField("currentFloorNo", 1);
            dataHolder.SetField("currentFloor", currentFloor);

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
            IChainEnvironmentDataHolder dataHolder = environment.GetDataHolder(typeof(string).AssemblyQualifiedName);;

            List<FloorDataFrame<string>> floorDataFrames = dataHolder.GetField<List<FloorDataFrame<string>>>("floorDataFrames");
            var currentFloor = new FloorDataFrame<string>();
            floorDataFrames.Add(currentFloor);
            dataHolder.SetField("currentFloorNo", 1);
            dataHolder.SetField("currentFloor", currentFloor);

            string variableName = "t";

            Assert.ThrowsException<frEnvironment.UndefinedVariableException>(() => {
                environment.GetValue(variableName);
            });
        }
        //チェーン
        //同階層に定義のある変数の値を取得しようとしたとき
        [TestMethod()]
        public void GetValueTest4() {
            frEnvironment upstairEnvironment = new frEnvironment();
            IChainEnvironmentDataHolder upstairdataHolder = upstairEnvironment.GetDataHolder(typeof(string).AssemblyQualifiedName);;

            FloorDataFrame<string> currentFloor = upstairdataHolder.GetField<FloorDataFrame<string>>("currentFloor");
            string variableName = "t";
            string value = "t-value";

            frEnvironment environment = new frEnvironment();
            environment.SetUpstairEnvironment_LooseConnection(upstairEnvironment);

            currentFloor.Variables.Add(variableName, value);

            Assert.AreEqual(value, environment.GetValue(variableName));

        }
        //チェーン＆マルチバンド
        //同階層に定義のある変数の値を取得しようとしたとき
        [TestMethod()]
        public void GetValueTest4_1() {
            frEnvironment upstairEnvironment = new frEnvironment();
            IChainEnvironmentDataHolder upstairdataHolder = upstairEnvironment.GetDataHolder(typeof(string).AssemblyQualifiedName);;

            FloorDataFrame<string> currentFloor = upstairdataHolder.GetField<FloorDataFrame<string>>("currentFloor");
            string variableName = "t";
            string value = "t-value";

            frEnvironment environment = new frEnvironment();
            MultiBandUpstairEnvironment multiBand = new MultiBandUpstairEnvironment(environment);
            multiBand.UpstairEnvironments.Add(upstairEnvironment);

            currentFloor.Variables.Add(variableName, value);

            Assert.AreEqual(value, environment.GetValue(variableName));

        }
        //int型
        //同階層に定義のある変数の値を取得しようとしたとき
        [TestMethod()]
        public void GetValueTest5() {
            frEnvironment upstairEnvironment = new frEnvironment();

            IChainEnvironmentDataHolder upstairdataHolder = upstairEnvironment.GetDataHolder(typeof(int).AssemblyQualifiedName);

            FloorDataFrame<int> currentFloor = upstairdataHolder.GetField<FloorDataFrame<int>>("currentFloor");
            string variableName = "t";
            int value = 10;

            frEnvironment environment = new frEnvironment();
            environment.SetUpstairEnvironment_LooseConnection(upstairEnvironment);

            currentFloor.Variables.Add(variableName, value);

            Assert.AreEqual(value, environment.GetValue(typeof(int), variableName));
            //Assert.AreEqual(value, environment.GetValue<int>(variableName));

        }
        //int型
        //同階層に定義のある変数の値を取得しようとしたとき
        [TestMethod()]
        public void GetValueTest6() {
            frEnvironment upstairEnvironment = new frEnvironment();

            IChainEnvironmentDataHolder upstairdataHolder = upstairEnvironment.GetDataHolder(typeof(Dictionary<string, int>).AssemblyQualifiedName);

            FloorDataFrame<Dictionary<string, int>> currentFloor = upstairdataHolder.GetField<FloorDataFrame<Dictionary<string, int>>>("currentFloor");
            string variableName = "t";
            string dkey = "key";
            int dvalue = 12;
            Dictionary<string, int> value = new Dictionary<string, int>();

            value.Add(dkey, dvalue);

            frEnvironment environment = new frEnvironment();
            environment.SetUpstairEnvironment_LooseConnection(upstairEnvironment);

            currentFloor.Variables.Add(variableName, value);

            Assert.AreEqual(value, environment.GetValue(typeof(Dictionary<string, int>), variableName)); 
            foreach (KeyValuePair<string, int> keyValuePair in currentFloor.Variables[variableName]) {
                Assert.AreEqual(dkey, keyValuePair.Key);
                Assert.AreEqual(dvalue, keyValuePair.Value);
            }

        }
        #endregion

        #region(SetValue)
        //現階層が大域環境で、すでに変数の定義がある場合
        [TestMethod()]
        public void SetValueTest() {
            frEnvironment environment = new frEnvironment();
            IChainEnvironmentDataHolder dataHolder = environment.GetDataHolder(typeof(string).AssemblyQualifiedName);;

            FloorDataFrame<string> currentFloor = dataHolder.GetField<FloorDataFrame<string>>("currentFloor");
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
            IChainEnvironmentDataHolder dataHolder = environment.GetDataHolder(typeof(string).AssemblyQualifiedName);;

            List<FloorDataFrame<string>> floorDataFrames = dataHolder.GetField<List<FloorDataFrame<string>>>("floorDataFrames");
            var currentFloor = new FloorDataFrame<string>();
            floorDataFrames.Add(currentFloor);
            dataHolder.SetField("currentFloorNo", 1);
            dataHolder.SetField("currentFloor", currentFloor);

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
            IChainEnvironmentDataHolder dataHolder = environment.GetDataHolder(typeof(string).AssemblyQualifiedName);;

            FloorDataFrame<string> currentFloor = dataHolder.GetField<FloorDataFrame<string>>("currentFloor");
            string variableName = "t";
            string value = "t-value";

            environment.SetValue(variableName, value);

            Assert.AreEqual(value, currentFloor.Variables[variableName]);
        }
        //現階層が非大域環境で、現階層から大域環境まで含めて変数の定義がない場合
        [TestMethod()]
        public void SetValueTest3() {
            frEnvironment environment = new frEnvironment();
            IChainEnvironmentDataHolder dataHolder = environment.GetDataHolder(typeof(string).AssemblyQualifiedName);;

            List<FloorDataFrame<string>> floorDataFrames = dataHolder.GetField<List<FloorDataFrame<string>>>("floorDataFrames");
            var floorAbove = floorDataFrames[0];
            var currentFloor = new FloorDataFrame<string>();
            floorDataFrames.Add(currentFloor);
            dataHolder.SetField("currentFloorNo", 1);
            dataHolder.SetField("currentFloor", currentFloor);

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
            IChainEnvironmentDataHolder dataHolder = environment.GetDataHolder(typeof(string).AssemblyQualifiedName);;

            List<FloorDataFrame<string>> floorDataFrames = dataHolder.GetField<List<FloorDataFrame<string>>>("floorDataFrames");
            floorDataFrames.Add(new FloorDataFrame<string>());
            floorDataFrames.Add(new FloorDataFrame<string>());
            var globalfloor = floorDataFrames[0];
            var floorAbove = floorDataFrames[1];
            var currentFloor = floorDataFrames[2];
            dataHolder.SetField("currentFloorNo", 2);
            dataHolder.SetField("currentFloor", currentFloor);

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
        //現階層が大域環境で、現環境に変数の定義がなくまたチェン先に定義ありの場合
        [TestMethod()]
        public void SetValueTest5() {
            frEnvironment upstairEnvironment = new frEnvironment();
            IChainEnvironmentDataHolder upstairdataHolder = upstairEnvironment.GetDataHolder(typeof(string).AssemblyQualifiedName);;

            FloorDataFrame<string> currentFloor = upstairdataHolder.GetField<FloorDataFrame<string>>("currentFloor");
            string variableName = "t";
            string oldValue = "old-value";
            string newValue = "new-value";

            frEnvironment environment = new frEnvironment();
            environment.SetUpstairEnvironment_LooseConnection(upstairEnvironment);

            //チェン先に変数を追加
            currentFloor.Variables.Add(variableName, oldValue);

            environment.SetValue(variableName, newValue);

            Assert.AreEqual(newValue, currentFloor.Variables[variableName]);
        }
        //マルチバンド
        //現階層が大域環境で、現環境に変数の定義がなくまたチェン先に定義ありの場合
        [TestMethod()]
        public void SetValueTest5_1() {
            frEnvironment upstairEnvironment = new frEnvironment();
            IChainEnvironmentDataHolder upstairdataHolder = upstairEnvironment.GetDataHolder(typeof(string).AssemblyQualifiedName);;

            FloorDataFrame<string> currentFloor = upstairdataHolder.GetField<FloorDataFrame<string>>("currentFloor");
            string variableName = "t";
            string oldValue = "old-value";
            string newValue = "new-value";

            frEnvironment environment = new frEnvironment();
            MultiBandUpstairEnvironment multiBand = new MultiBandUpstairEnvironment(environment);
            multiBand.UpstairEnvironments.Add(upstairEnvironment);

            //チェン先に変数を追加
            currentFloor.Variables.Add(variableName, oldValue);

            environment.SetValue<string>(variableName, newValue);

            Assert.AreEqual(newValue, currentFloor.Variables[variableName]);
        }
        //int型
        //現階層が大域環境で、現環境に変数の定義がなくまたチェン先に定義ありの場合
        [TestMethod()]
        public void SetValueTest6() {
            frEnvironment upstairEnvironment = new frEnvironment();
            IChainEnvironmentDataHolder upstairdataHolder = upstairEnvironment.GetDataHolder(typeof(int).AssemblyQualifiedName);

            FloorDataFrame<int> currentFloor = upstairdataHolder.GetField<FloorDataFrame<int>>("currentFloor");
            string variableName = "t";
            int oldValue = 10;
            int newValue = 15;

            frEnvironment environment = new frEnvironment();
            environment.SetUpstairEnvironment_LooseConnection(upstairEnvironment);

            //チェン先に変数を追加
            currentFloor.Variables.Add(variableName, oldValue);

            environment.SetValue(typeof(int), variableName, newValue);

            Assert.AreEqual(newValue, currentFloor.Variables[variableName]);
        }
        //Dictionary型
        //現階層が大域環境で、現環境に変数の定義がなくまたチェン先に定義ありの場合
        [TestMethod()]
        public void SetValueTest7() {
            frEnvironment upstairEnvironment = new frEnvironment();
            IChainEnvironmentDataHolder upstairdataHolder = upstairEnvironment.GetDataHolder(typeof(Dictionary<string,int>).AssemblyQualifiedName);

            FloorDataFrame<Dictionary<string, int>> currentFloor = upstairdataHolder.GetField<FloorDataFrame<Dictionary<string, int>>>("currentFloor");
            string variableName = "t";
            string dkey = "key";
            int dvalue = 12;
            Dictionary<string, int> oldValue = new Dictionary<string, int>();
            Dictionary<string, int> newValue = new Dictionary<string, int>();

            newValue.Add(dkey, dvalue);

            frEnvironment environment = new frEnvironment();
            environment.SetUpstairEnvironment_LooseConnection(upstairEnvironment);

            //チェン先に変数を追加
            currentFloor.Variables.Add(variableName, oldValue);

            environment.SetValue(typeof(Dictionary<string, int>), variableName, newValue);

            Assert.AreEqual(newValue, currentFloor.Variables[variableName]);
            foreach(KeyValuePair<string,int> keyValuePair in currentFloor.Variables[variableName]) {
                Assert.AreEqual(dkey, keyValuePair.Key);
                Assert.AreEqual(dvalue, keyValuePair.Value);
            }
        }




        #endregion

        #region(CreateOrSetValue_Local)
        //現階層が大域環境で、すでに変数の定義がある場合
        [TestMethod()]
        public void CreateOrSetValue_LocalTest() {
            frEnvironment environment = new frEnvironment();
            IChainEnvironmentDataHolder dataHolder = environment.GetDataHolder(typeof(string).AssemblyQualifiedName); ;

            FloorDataFrame<string> currentFloor = dataHolder.GetField<FloorDataFrame<string>>("currentFloor");
            string variableName = "t";
            string oldValue = "old-value";
            string newValue = "new-value";

            currentFloor.Variables.Add(variableName, oldValue);

            environment.CreateOrSetValue_Local(variableName, newValue);

            Assert.AreEqual(newValue, currentFloor.Variables[variableName]);
        }

        //現階層が非大域環境で、現階層にすでに変数の定義がある場合
        [TestMethod()]
        public void CreateOrSetValue_LocalTest1() {
            frEnvironment environment = new frEnvironment();
            IChainEnvironmentDataHolder dataHolder = environment.GetDataHolder(typeof(string).AssemblyQualifiedName); ;

            List<FloorDataFrame<string>> floorDataFrames = dataHolder.GetField<List<FloorDataFrame<string>>>("floorDataFrames");
            var currentFloor = new FloorDataFrame<string>();
            floorDataFrames.Add(currentFloor);
            dataHolder.SetField("currentFloorNo", 1);
            dataHolder.SetField("currentFloor", currentFloor);

            string variableName = "t";
            string oldValue = "old-value";
            string newValue = "new-value";

            currentFloor.Variables.Add(variableName, oldValue);

            environment.CreateOrSetValue_Local(variableName, newValue);

            Assert.AreEqual(newValue, currentFloor.Variables[variableName]);
        }
        //現階層が大域環境で、変数の定義がない場合
        [TestMethod()]
        public void CreateOrSetValue_LocalTest2() {
            frEnvironment environment = new frEnvironment();
            IChainEnvironmentDataHolder dataHolder = environment.GetDataHolder(typeof(string).AssemblyQualifiedName);;

            FloorDataFrame<string> currentFloor = dataHolder.GetField<FloorDataFrame<string>>("currentFloor");
            string variableName = "t";
            string value = "t-value";

            environment.CreateOrSetValue_Local(variableName, value);

            Assert.AreEqual(value, currentFloor.Variables[variableName]);
        }
        //現階層が非大域環境で、現階層から大域環境まで含めて変数の定義がない場合
        [TestMethod()]
        public void CreateOrSetValue_LocalTest3() {
            frEnvironment environment = new frEnvironment();
            IChainEnvironmentDataHolder dataHolder = environment.GetDataHolder(typeof(string).AssemblyQualifiedName);;

            List<FloorDataFrame<string>> floorDataFrames = dataHolder.GetField<List<FloorDataFrame<string>>>("floorDataFrames");
            var floorAbove = floorDataFrames[0];
            var currentFloor = new FloorDataFrame<string>();
            floorDataFrames.Add(currentFloor);
            dataHolder.SetField("currentFloorNo", 1);
            dataHolder.SetField("currentFloor", currentFloor);

            string variableName = "t";
            string value = "t-value";

            environment.CreateOrSetValue_Local(variableName, value);

            Assert.AreEqual(value, currentFloor.Variables[variableName]);
            Assert.AreEqual(false, floorAbove.Variables.ContainsKey(variableName));
        }
        //大域環境に定義あり、第一層に定義あり、第二層が現階層で定義なしの場合
        [TestMethod()]
        public void CreateOrSetValue_LocalTest4() {
            frEnvironment environment = new frEnvironment();
            IChainEnvironmentDataHolder dataHolder = environment.GetDataHolder(typeof(string).AssemblyQualifiedName);;

            List<FloorDataFrame<string>> floorDataFrames = dataHolder.GetField<List<FloorDataFrame<string>>>("floorDataFrames");
            floorDataFrames.Add(new FloorDataFrame<string>());
            floorDataFrames.Add(new FloorDataFrame<string>());
            var globalfloor = floorDataFrames[0];
            var floorAbove = floorDataFrames[1];
            var currentFloor = floorDataFrames[2];
            dataHolder.SetField("currentFloorNo", 2);
            dataHolder.SetField("currentFloor", currentFloor);

            string variableName = "t";
            string oldValue_0 = "old-value_0";
            string oldValue_1 = "old-value_1";
            string newValue = "new-value";

            globalfloor.Variables.Add(variableName, oldValue_0);
            floorAbove.Variables.Add(variableName, oldValue_1);

            environment.CreateOrSetValue_Local(variableName, newValue);

            Assert.AreEqual(oldValue_0, globalfloor.Variables[variableName]);
            Assert.AreEqual(oldValue_1, floorAbove.Variables[variableName]);
            Assert.AreEqual(newValue, currentFloor.Variables[variableName]);
        }


        #endregion

        #region(Exists)
        //現階層が大域環境の場合
        [TestMethod()]
        public void ExistsTest() {
            frEnvironment environment = new frEnvironment();
            IChainEnvironmentDataHolder dataHolder = environment.GetDataHolder(typeof(string).AssemblyQualifiedName);;

            FloorDataFrame<string> currentFloor = dataHolder.GetField<FloorDataFrame<string>>("currentFloor");
            string variableName_0 = "t_0";
            string variableName_1 = "t_1";
            string value = "value";

            currentFloor.Variables.Add(variableName_0, value);

            Assert.AreEqual(true, environment.Exists(variableName_0));
            Assert.AreEqual(false, environment.Exists(variableName_1));
        }

        //現階層が非大域環境の場合
        [TestMethod()]
        public void ExistsTest1() {
            frEnvironment environment = new frEnvironment();
            IChainEnvironmentDataHolder dataHolder = environment.GetDataHolder(typeof(string).AssemblyQualifiedName);;

            List<FloorDataFrame<string>> floorDataFrames = dataHolder.GetField<List<FloorDataFrame<string>>>("floorDataFrames");
            var floorAbove = floorDataFrames[0];
            var currentFloor = new FloorDataFrame<string>();
            floorDataFrames.Add(currentFloor);
            dataHolder.SetField("currentFloorNo", 1);
            dataHolder.SetField("currentFloor", currentFloor);

            string variableName_0 = "t_0";
            string variableName_1 = "t_1";
            string value = "value";

            floorAbove.Variables.Add(variableName_0, value);

            Assert.AreEqual(true, environment.Exists(variableName_0));
            Assert.AreEqual(false, environment.Exists(variableName_1));
        }

        //チェーン
        //同階層に定義のある変数の値を取得しようとしたとき
        [TestMethod()]
        public void ExistsTest2() {
            frEnvironment upstairEnvironment = new frEnvironment();
            IChainEnvironmentDataHolder upstairdataHolder = upstairEnvironment.GetDataHolder(typeof(string).AssemblyQualifiedName); ;

            FloorDataFrame<string> currentFloor = upstairdataHolder.GetField<FloorDataFrame<string>>("currentFloor");
            string variableName_0 = "t_0";
            string variableName_1 = "t_1";
            string value = "value";

            frEnvironment environment = new frEnvironment();
            environment.SetUpstairEnvironment_LooseConnection(upstairEnvironment);

            currentFloor.Variables.Add(variableName_0, value);

            Assert.AreEqual(true, environment.Exists(variableName_0));
            Assert.AreEqual(false, environment.Exists(variableName_1));

        }
        #endregion

        #region(Down)
        [TestMethod()]
        public void DownTest() {
            frEnvironment environment = new frEnvironment();
            IChainEnvironmentDataHolder dataHolder = environment.GetDataHolder(typeof(string).AssemblyQualifiedName);;

            FloorDataFrame<string> oldCurrentFloor = dataHolder.GetField<FloorDataFrame<string>>("currentFloor");
            int oldCurrentFloorNo = dataHolder.GetField<int>("currentFloorNo");
            List<string> returnValues = new List<string> { "rv0", "rv1" };
            List<string> arguments = new List<string> { "arg0", "arg1" };

            environment.Down(returnValues, arguments);

            FloorDataFrame<string> newCurrentFloor = dataHolder.GetField<FloorDataFrame<string>>("currentFloor");
            int newCurrentFloorNo = dataHolder.GetField<int>("currentFloorNo");

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
            IChainEnvironmentDataHolder dataHolder = environment.GetDataHolder(typeof(string).AssemblyQualifiedName);;

            FloorDataFrame<string> oldCurrentFloor = dataHolder.GetField<FloorDataFrame<string>>("currentFloor");
            int oldCurrentFloorNo = dataHolder.GetField<int>("currentFloorNo");
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

            FloorDataFrame<string> newCurrentFloor = dataHolder.GetField<FloorDataFrame<string>>("currentFloor");
            int newCurrentFloorNo = dataHolder.GetField<int>("currentFloorNo");

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
            IChainEnvironmentDataHolder dataHolder = environment.GetDataHolder(typeof(string).AssemblyQualifiedName);;

            FloorDataFrame<string> oldCurrentFloor = dataHolder.GetField<FloorDataFrame<string>>("currentFloor");
            int oldCurrentFloorNo = dataHolder.GetField<int>("currentFloorNo");
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