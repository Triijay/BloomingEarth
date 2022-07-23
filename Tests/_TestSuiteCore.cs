using System;
using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace Tests
{
    public class _TestSuiteCore {

        public InitGame Game;

        [SetUp]
        public void SetUp() {
        }

        [TearDown]
        public void TearDown() {
        }


        #region IdleNum


        [Test]
        public void UnitTestIdlenumConstructorsAndIsValidIdleNum() {

            double testD = 2.2;
            string testSuf1 = "K";
            string testSuf2 = "M";
            string testSuf3 = "AA";


            // double Constructor
            IdleNum idletest = new IdleNum(testD);
            IdleNum idletest2 = new IdleNum(testD, testSuf1);
            IdleNum idletest6 = new IdleNum(testD, testSuf3);

            // int Constructor
            IdleNum idletest3 = new IdleNum(0);
            IdleNum idletest4 = new IdleNum(1);
            IdleNum idletest5 = new IdleNum(1, testSuf2);

            // CopyConstructor
            IdleNum idletestCopy2 = new IdleNum(idletest2);

            // Check if no constructor Error occur
            Assert.IsNotNull(idletest);
            Assert.IsNotNull(idletest2);
            Assert.IsNotNull(idletest3);
            Assert.IsNotNull(idletest4);
            Assert.IsNotNull(idletest5);
            Assert.IsNotNull(idletest6);

            Assert.AreEqual(idletest2, idletestCopy2);


            //* Check if they are valid IdleNums

            // Amounts
            Assert.AreEqual(testD, idletest.getAmount());
            Assert.AreEqual(testD, idletest2.getAmount());
            Assert.AreEqual(testD , idletest6.getAmount());
            Assert.AreEqual(0, idletest3.getAmount());
            Assert.AreEqual(1, idletest4.getAmount());
            Assert.AreEqual(1, idletest5.getAmount());

            // Suffixes
            Assert.AreEqual("", idletest.getSuffix());
            Assert.AreEqual(testSuf1, idletest2.getSuffix());
            Assert.AreEqual(testSuf3, idletest6.getSuffix());
            Assert.AreEqual("", idletest3.getSuffix());
            Assert.AreEqual("", idletest4.getSuffix());
            Assert.AreEqual(testSuf2, idletest5.getSuffix());

        }



        [Test]
        public void UnitTestIdleNumSuffixToInt() {

            double testD = 2.2;
            string testSuf1 = "K";
            string testSuf11 = "M";
            string testSuf12 = "B";
            string testSuf13 = "T";
            string testSuf3 = "AA";
            string testSuf4 = "BC";


            string testSufErr1 = "Z"; // Because Z is not part of our Special Chars
            string testSufErr2 = "Ziau"; // Because more than one char
            string testSufErr3 = "Miau"; // Because starts with Valid M
            string testSufErr4 = "Miau XXX"; // Because emptySpace


            // Valid Conctructors
            IdleNum idletest = new IdleNum(testD);
            IdleNum idletest1 = new IdleNum(testD, testSuf1);
            IdleNum idletest11 = new IdleNum(testD, testSuf11);
            IdleNum idletest12 = new IdleNum(testD, testSuf12);
            IdleNum idletest13 = new IdleNum(testD, testSuf13);
            IdleNum idletest3 = new IdleNum(testD, testSuf3);
            IdleNum idletest4 = new IdleNum(testD, testSuf4);

            // error Constructor TODO: Prevent more than 2-3 Chars at Suffix
            IdleNum idletestErr1 = new IdleNum(1, testSufErr1);
            IdleNum idletestErr2 = new IdleNum(1, testSufErr2);
            IdleNum idletestErr3 = new IdleNum(1, testSufErr3);
            IdleNum idletestErr4 = new IdleNum(1, testSufErr4);
            // TODO: Prevent construction of negative IdleNum

            // suffixToInt
            Assert.AreEqual(0, idletest.suffixToInt());
            Assert.AreEqual(1, idletest1.suffixToInt());
            Assert.AreEqual(2, idletest11.suffixToInt());
            Assert.AreEqual(3, idletest12.suffixToInt());
            Assert.AreEqual(4, idletest13.suffixToInt());
            Assert.AreEqual(5, idletest3.suffixToInt()); //AA
            Assert.Less(5, idletest4.suffixToInt()); //BC

            // Errors
            Assert.AreEqual(0,  idletestErr1.suffixToInt());
            Assert.AreEqual(0, idletestErr2.suffixToInt());
            Assert.AreEqual(0, idletestErr3.suffixToInt());
            Assert.AreEqual(0, idletestErr4.suffixToInt());
        }


        [Test]
        public void UnitTestIdleNumToRoundedString() {

            IdleNum idletest1 = new IdleNum(1);
            IdleNum idletest2 = new IdleNum(1.2, "K");

            // intToSuffix Test
            Assert.AreEqual("1", idletest1.toRoundedString(0));
            Assert.AreEqual("1 K", idletest2.toRoundedString(0));
            Assert.AreEqual("1 K", idletest2.toRoundedString(0));
            Assert.AreEqual("1.2 K", idletest2.toRoundedString(1));
            Assert.AreEqual("1.20 K", idletest2.toRoundedString());
            Assert.AreEqual("1.20 K", idletest2.toRoundedString(2));

        }


        [Test]
        public void UnitTestIdleNumIntToSuffix() {

            IdleNum idletest = new IdleNum(1.2);

            // intToSuffix Test
            Assert.AreEqual("", idletest.intToSuffix(0));
            Assert.AreEqual("K", idletest.intToSuffix(1));
            Assert.AreEqual("M", idletest.intToSuffix(2));
            Assert.AreEqual("B", idletest.intToSuffix(3));
            Assert.AreEqual("T", idletest.intToSuffix(4));
            Assert.AreEqual("AA", idletest.intToSuffix(5));
        }


        [Test]
        public void UnitTestIdleNumManipulateSuffix() {

            IdleNum idletest = new IdleNum(1000, "K");
            IdleNum idletextCopy = new IdleNum(idletest);

            idletest.incrementSuffix();

            // intToSuffix Test
            Assert.AreEqual(1, idletest.getAmount());
            Assert.AreEqual("M", idletest.getSuffix());

            idletest.decrementSuffix();

            // intToSuffix Test
            Assert.AreEqual(1000, idletest.getAmount());
            Assert.AreEqual("K", idletest.getSuffix());
            Assert.AreEqual(idletextCopy, idletest);

        }

        [Test]
        public void UnitTestIdleNumArithmeticOperators() {

            IdleNum idletest = new IdleNum(1);
            IdleNum idletest1 = new IdleNum(1, "K");
            IdleNum idletest2 = new IdleNum(2.3, "K");
            IdleNum idletest3 = new IdleNum(999, "K");

            IdleNum idletest4 = new IdleNum(1, "M");
            IdleNum idletest5 = new IdleNum(1, "AA");
            IdleNum idletest6 = new IdleNum(2);

            // +
            Assert.AreEqual(new IdleNum(2), idletest + idletest);
            Assert.AreEqual(new IdleNum(2, "K"), idletest1 + idletest1);
            Assert.AreEqual(new IdleNum(3.3, "K"), idletest1 + idletest2);
            Assert.AreEqual(new IdleNum(1, "M"), idletest1 + idletest3); // Automatic increment suffix
            // + Special (Two different Suffixes)
            Assert.AreEqual(new IdleNum(1.999, "M"), idletest3 + idletest4); // A
            Assert.AreEqual(new IdleNum(1.001, "K"), idletest1 + idletest); // B
            Assert.AreEqual(new IdleNum(1.999, "M"), idletest4 + idletest3); // A Turned around
            Assert.AreEqual(new IdleNum(1.001, "K"), idletest + idletest1); // B Turned around
            // + Borders (SuffixDiff > 4) should not affect the larger IdleNum
            Assert.AreEqual(new IdleNum(1, "AA"), idletest + idletest5);
            Assert.AreEqual(new IdleNum(1, "AA"), idletest5 + idletest);
            Assert.AreNotEqual(new IdleNum(1, "AA"), idletest1 + idletest5);


            // -
            // Note At Substraction we are using String-Comparison because of Floating-Point Arithmetic -> Rounding Errors (Bit unprecise)
            Assert.AreEqual(new IdleNum(0), idletest - idletest);
            Assert.AreEqual(new IdleNum(0), idletest1 - idletest1);
            Assert.AreEqual(new IdleNum(1).toRoundedString(15), (idletest6 - idletest).toRoundedString(15));

            Assert.AreEqual(new IdleNum(1.3, "K").toRoundedString(15), (idletest2 - idletest1).toRoundedString(15));

            // - Special (Two different Suffixes)
            Assert.AreEqual(new IdleNum(1, "K").toRoundedString(15), (idletest4 - idletest3).toRoundedString(15)); // Automatic decrement suffix
            Assert.AreEqual(new IdleNum(999).toRoundedString(15), (idletest1 - idletest).toRoundedString(15)); // 1 without suffix

            // - Borders (SuffixDiff > 4) should not affect the larger IdleNum
            Assert.AreEqual(new IdleNum(1, "AA").toRoundedString(15), (idletest5 - idletest).toRoundedString(15));
            Assert.AreNotEqual(new IdleNum(1, "AA").toRoundedString(15), (idletest5 - idletest1).toRoundedString(15));

            // - Negative Result should return a 0 IdleNum
            Assert.AreEqual(new IdleNum(0), idletest1 - idletest2);


            // * 
            Assert.AreEqual(new IdleNum(1), idletest * 1);
            Assert.AreEqual(new IdleNum(2), idletest * 2);
            Assert.AreEqual(new IdleNum(4.6, "K"), idletest2 * 2);
            Assert.AreEqual(new IdleNum(2, "K"), idletest6 * 1000); // Auto decrement
            Assert.AreEqual(new IdleNum(1, "B"), idletest1 * 1000000); // Auto decrement twice
            Assert.AreEqual(new IdleNum(0), idletest1 * 0); // 0
            // TODO: Prevent Negative Factor

            // /
            Assert.AreEqual(new IdleNum(1), idletest / 1);
            Assert.AreEqual(new IdleNum(0.5), idletest / 2, "Result: " + (idletest / 2).toRoundedString() );
            Assert.AreEqual(new IdleNum(1), idletest6 / 2);
            Assert.AreEqual(new IdleNum(1), idletest1 / 1000); // Auto decrement
            // TODO: Prevent Negative Factor
            // TODO: Prevent 0 as Factor


        }


        [Test]
        public void UnitTestIdleNumComparingOperators() {

            IdleNum idletest = new IdleNum(1);
            IdleNum idletestCopy = new IdleNum(1);
            IdleNum idletest1 = new IdleNum(1, "K");
            IdleNum idletest2 = new IdleNum(2.3, "K");
            IdleNum idletest3 = new IdleNum(999, "K");

            IdleNum idletest4 = new IdleNum(1, "M");
            IdleNum idletest5 = new IdleNum(1, "AA");
            IdleNum idletest6 = new IdleNum(2);

            // Equals
            Assert.IsFalse(idletest.Equals(idletest1));
            Assert.IsFalse(idletest.Equals(null));
            Assert.IsFalse(idletest.Equals(1));
            Assert.IsTrue(idletest.Equals(idletestCopy));
            Assert.IsTrue(idletest.Equals(idletest));

            // ==
            Assert.IsTrue(idletest == idletestCopy); // Copy of IdleNum
            Assert.IsFalse(idletest == idletest1); // Different Suffix same Amount
            Assert.IsFalse(idletest1 == idletest2); // Same Suffix different Amount
            Assert.IsFalse(idletest == idletest6); // Without Suffix different Amount

            // !=
            Assert.IsFalse(idletest != idletestCopy); // Copy of IdleNum
            Assert.IsTrue(idletest != idletest1); // Different Suffix same Amount
            Assert.IsTrue(idletest1 != idletest2); // Same Suffix different Amount
            Assert.IsTrue(idletest != idletest6); // Without Suffix different Amount


            // >=
            Assert.IsTrue(idletest >= idletestCopy); // Copy of IdleNum
            Assert.IsFalse(idletest >= idletest6); // Same Suffix different Amount
            Assert.IsTrue(idletest6 >= idletest); // Same Suffix different Amount 
            Assert.IsFalse(idletest >= idletest1); // Different Suffix same Amount
            Assert.IsTrue(idletest1 >= idletest); // Different Suffix same Amount

            // >
            Assert.IsFalse(idletest > idletestCopy); // Copy of IdleNum
            Assert.IsFalse(idletest > idletest6); // Same Suffix different Amount
            Assert.IsTrue(idletest6 > idletest); // Same Suffix different Amount 
            Assert.IsFalse(idletest > idletest1); // Different Suffix same Amount
            Assert.IsTrue(idletest1 > idletest); // Different Suffix same Amount


            // <=
            Assert.IsTrue(idletest <= idletestCopy); // Copy of IdleNum
            Assert.IsTrue(idletest <= idletest6); // Same Suffix different Amount
            Assert.IsFalse(idletest6 <= idletest); // Same Suffix different Amount 
            Assert.IsTrue(idletest <= idletest1); // Different Suffix same Amount
            Assert.IsFalse(idletest1 <= idletest); // Different Suffix same Amount

            // <
            Assert.IsFalse(idletest < idletestCopy); // Copy of IdleNum
            Assert.IsTrue(idletest < idletest6); // Same Suffix different Amount
            Assert.IsFalse(idletest6 < idletest); // Same Suffix different Amount 
            Assert.IsTrue(idletest < idletest1); // Different Suffix same Amount
            Assert.IsFalse(idletest1 < idletest); // Different Suffix same Amount
        }

        #endregion


        #region Affector



        [Test]
        public void UnitTestAffectorConstructorAndIsValidAffector() {

            IdleNum idleNum = new IdleNum(3);
            IdleNum idleNum2 = new IdleNum(4);


            Affector<int> affector1 = new Affector<int>("affector1", 1);
            Affector<double> affector2 = new Affector<double>("affector2", 1.1);
            Affector<IdleNum> affector3 = new Affector<IdleNum>("affector3", idleNum);
            Affector<float> affector4 = new Affector<float>("affector4", 2.1f);

            Assert.IsNotNull(affector1);
            Assert.IsNotNull(affector2);
            Assert.IsNotNull(affector3);
            Assert.IsNotNull(affector4);

            //getID
            Assert.AreEqual("affector1", affector1.getID());
            Assert.AreEqual("affector2", affector2.getID());
            Assert.AreEqual("affector3", affector3.getID());
            Assert.AreEqual("affector4", affector4.getID());

            // getAffection
            Assert.AreEqual(1, affector1.getAffection());
            Assert.AreEqual(1.1, affector2.getAffection());
            Assert.AreEqual(idleNum, affector3.getAffection());
            Assert.AreEqual(2.1f, affector4.getAffection());

            //setAffection
            affector1.setAffection(2);
            affector2.setAffection(2.2);
            affector3.setAffection(idleNum2);
            affector4.setAffection(6.6f);
            Assert.AreEqual(2, affector1.getAffection());
            Assert.AreEqual(2.2, affector2.getAffection());
            Assert.AreEqual(idleNum2, affector3.getAffection());
            Assert.AreEqual(6.6f, affector4.getAffection());

        }

        #endregion


        #region EnviGlass

        [Test]
        public void UnitTestEnviGlassConstruction() {

            // Constructor
            EnviGlass myEnvi = new EnviGlass();

            // ConstructorTest
            Assert.IsNotNull(myEnvi);

            //getEnviValue
            Assert.AreEqual(0, myEnvi.getEnviValue());

            // Create Affectors and attach them to myEnvi
            Affector<float> affector1 = new Affector<float>("affector1", 1.1f);
            Affector<float> affector2 = new Affector<float>("affector2", 1.2f);
            Affector<float> affector3 = new Affector<float>("affector3", 1.3f);
            myEnvi.addAffector(affector1);
            myEnvi.addAffector(affector2);
            myEnvi.addAffector(affector3);

            // getEnviValue with Affectors
            Assert.AreEqual(3.6f, myEnvi.getEnviValue(), 0.0001f);
            // -> updateEnviValue is covered

            // dont add Affector twice (still 3.6)
            myEnvi.addAffector(affector1);
            Assert.AreEqual(3.6f, myEnvi.getEnviValue(), 0.0001f);
            // -> isAlreadyAffector is covered when we are reaching this line

        }

        [Test]
        public void UnitTestEnviGlassManipulation() {

            // Constructor
            EnviGlass myEnvi = new EnviGlass();
            Affector<float> affector1 = new Affector<float>("affector1", 1.1f);
            Affector<float> affector2 = new Affector<float>("affector2", 1.2f);
            Affector<float> affector3 = new Affector<float>("affector3", 1.3f);
            myEnvi.addAffector(affector1);
            myEnvi.addAffector(affector2);
            myEnvi.addAffector(affector3);

            // getEnviValue with Affectors
            Assert.AreEqual(3.6f, myEnvi.getEnviValue(), 0.0001f);

            // updateSpecificAffector
            myEnvi.updateSpecificAffector(affector1.getID(), 10f);

            Assert.AreEqual(10f, affector1.getAffection(), 0.0001f);
            Assert.AreEqual(12.5f, myEnvi.getEnviValue(), 0.0001f);
        }


        #endregion




    }
}
