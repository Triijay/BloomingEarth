using UnityEngine;
using System.Collections;
using System;

/// <summary>
/// IdleNum Class wich represents a number+suffix 
/// and has lots of Operators to Handle that combination
/// </summary>
[System.Serializable]
public class IdleNum {

    /// <summary>
    /// Amount of IdleNum in this IdleNum Instance (without the suffix)
    /// </summary>
    public double amount;

    /// <summary>
    /// Added String suffix for Example M for Million
    /// </summary>
    public string suffix;

    /// <summary>
    /// Amount of Special Chars (K, M, B, T)
    /// </summary>
    private const int SPECIAL_CHARS = 4;

    /// <summary>
    /// Constructor with Double and Suffix
    /// </summary>
    /// <param name="in_amount">Starting amount to be set</param>
    /// <param name="in_suffix">Starting suffix to be set</param>
    public IdleNum(double in_amount, string in_suffix = "") {
        amount = in_amount;
        suffix = in_suffix;
    }

    /// <summary>
    /// Constructor with Integer and Suffix
    /// </summary>
    /// <param name="in_amount">Starting amount to be set</param>
    /// <param name="in_suffix">Starting suffix to be set</param>
    public IdleNum(int in_amount, string in_suffix = "") : this((double)in_amount, in_suffix) { }

    /// <summary>
    /// Constructor with another IdleNum
    /// </summary>
    /// <param name="IdleNumToCopy">IdleNum to Copy</param>
    public IdleNum(IdleNum IdleNumToCopy) {
        amount = IdleNumToCopy.getAmount();
        suffix = IdleNumToCopy.getSuffix();
    }

    /// <summary>
    /// Get the rounded amount variable in string Format with suffix<br></br>
    /// rounding to 2 decimals is default, doesn't round when suffix is empty
    /// </summary>
    /// <param name="round"></param>
    /// <param name="forceRound">Returns the decimals even when suffix is empty (NOT for Users, only for debug)</param>
    /// <returns>String Containing rounded amount + suffix</returns>
    public string toRoundedString(int round = 2, bool forceRound = false) {
        if (suffix == "" && !forceRound) {
            return System.Math.Round(amount, 0) + "";
        } else {
            return System.Math.Round(amount, round).ToString("N"+round, System.Globalization.CultureInfo.CreateSpecificCulture("en-US")) + " " + suffix;
        }
    }

 // --- GETTERS/SETTERS   

    public double getAmount() {
        return amount;
    }

    public void setAmount(double am) {
        amount = am;
    }

    public string getSuffix() {
        return suffix;
    }

    public void setSuffix(string suff) {
        suffix = suff;
    }


    // --- END   GETTERS/SETTERS   


    /// <summary>
    /// Convert the Suffix as Integer value (K = 1, AB = 6)
    /// So we Can Compare it etc.
    /// </summary>
    /// <returns>Integer representing the suffix</returns>
    public int suffixToInt() {
        try {

            if (suffix.Length == 1) { // Special for K = 1, M = 2, B = 3, T = 4 and an empty String would be 0 
                if (suffix == "K") {
                    return 1;
                } else if (suffix == "M") {
                    return 2;
                } else if (suffix == "B") {
                    return 3;
                } else if (suffix == "T") {
                    return 4;
                } else { // SpecialChar not Recognized
                    // SATANS WORK
                    throw new Exception("SpecialChar not Recognized! @suffixToInt() (Was: " + suffix + ")");
                }
            } else if (suffix.Length == 2) { // AA +
                return (
                    // So Basicly this is our First Char Converted to ASCI and then Subtracted by 65 so we get A->0 B->1 etc. That * 26 so AA -> the First A is 0 B would be 26
                    (System.Convert.ToInt32(suffix[0]) - 65) * 26)
                    // This is our Second Char converted to ASCI and then Subtracted by 64 so we get A->1 B->2 etc. So AA -> the First A is 0 + Second A 1 = 1
                   + System.Convert.ToInt32(suffix[1]) - 64
                   // Now we Add the amount of Specialchars available (K, M, B, T) so we get AA = 1 + 4 = 5, AB = 6, AZ = 30, BA = 31
                   + SPECIAL_CHARS;
            } else if (suffix.Length == 0) {
                return 0;
            } else {
                // SATANS WORK
                throw new Exception("Suffix Length wasnt 0-2! (Was: " + suffix.Length +")");
            }
        } catch {
            return 0;
        }
    }

    /// <summary>
    /// Convert Integer to Suffix (1 = K, 6 = AB)
    /// </summary>
    /// <param name="integer">Integer to Convert</param>
    /// <returns>Suffix representing the Integer</returns>
    public string intToSuffix(int integer) {
        try {

            if(integer <= SPECIAL_CHARS) {
                // Special Chars Handle
                if (integer == 0) {
                    return "";
                } else if (integer == 1) {
                    return "K";
                } else if (integer == 2) {
                    return "M";
                } else if (integer == 3) {
                    return "B";
                } else if (integer == 4) {
                    return "T";
                } else {
                    // SATANS WORK
                    throw new Exception("SpecialChars is not Setup Correclty! @intToSuffix()");
                }

            } else {
                string suffix = "";

                // Calculate first Char in Suffix (So the A in AB) for that we have to divide the Int by 26 and add 65 ontop (25 / 26 = 0 + 65 == A, 26 / 26 = 1 + 65 = B)
                // We have to subtract 1 to change A to B @27 etc..
                suffix += System.Convert.ToChar(((integer - SPECIAL_CHARS - 1) / 26) + 65);

                // Now we look what reminder we have ( 31 - 4 - 1 = 26 % 26 = 0 = A, 30 - 4 - 1 = 25 = Z) 
                suffix += System.Convert.ToChar(((integer - SPECIAL_CHARS - 1) % 26) + 65);

                return suffix;
            }
        } catch {
            return "";
        }

    }


    /// <summary>
    /// Increment the Suffix by amount
    /// </summary>
    public void incrementSuffix() {
        amount = amount / 1000;

        // Just Convert Suffix to Int - Add 1 - Convert Back
        suffix = intToSuffix((suffixToInt() + 1));
    }

    /// <summary>
    /// Decrement the Suffix by amount
    /// </summary>
    public void decrementSuffix() {
        amount = amount * 1000;

        // Just Convert Suffix to Int - Subtract 1 - Convert Back
        suffix = intToSuffix((suffixToInt() - 1));
    }

    /// <summary>
    /// Custom + Operator (IdleNum + IdleNum)
    /// </summary>
    /// <param name="left">Left Coin</param>
    /// <param name="right">Right Coin</param>
    /// <returns>Left + Right</returns>
    public static IdleNum operator +(IdleNum left, IdleNum right) {
        IdleNum tmpIdleNum = new IdleNum(0);

        if (left.suffix == right.suffix) {
            // Same Suffix so we can just make a normal Addition
            tmpIdleNum.amount = left.amount + right.amount;

            // Take any of the 2 Suffixes cause they are the same anyway
            tmpIdleNum.suffix = left.suffix;
        } else {
            // We have to look wich side is Smaller and change the amount of that side to fit with the suffix of the other side

            // Convert the Suffixes to Integers so we can Compare them
            int leftSuffixInt = left.suffixToInt();
            int rightSuffixInt = right.suffixToInt();

            if (rightSuffixInt > leftSuffixInt) {
                // The left Suffix is Smaller than the right one

                // Save the Difference between right and left
                int diff = rightSuffixInt - leftSuffixInt;

                if (diff > 4) {
                    // Double can only hold down to 15 numbers after ',' so just ignore it and return the way bigger Coin
                    return right;
                } else {
                    // Here everything with Diff 1-4 gets
                    // We calculate 1000 to the power of diff so we can move the number accordingly
                    double divider = System.Math.Pow(1000.0, diff);

                    // Left is Smaller than right so we divide it and Add it to right
                    // a Divider of 1000 ^ 2 would move the Number by 6 to the right
                    tmpIdleNum.amount = (left.amount / divider) + right.amount;

                    // Take the Bigger Suffix
                    tmpIdleNum.suffix = right.suffix;
                }
            } else if (leftSuffixInt > rightSuffixInt) {
                // Same for the other way around

                int diff = leftSuffixInt - rightSuffixInt;

                if (diff > 4) {
                    return left;
                } else {
                    double divider = System.Math.Pow(1000.0, diff);
                    tmpIdleNum.amount = left.amount + (right.amount / divider);
                    tmpIdleNum.suffix = left.suffix;
                }
            }
        }

        // If we Reached the 1000 Mark with the previous Addition
        while (tmpIdleNum.amount >= 1000) {
            // We have to increment the Suffix
            tmpIdleNum.incrementSuffix();
        }

        return tmpIdleNum;
    }

    /// <summary>
    /// Custom - Operator (IdleNum - Coin)
    /// </summary>
    /// <param name="left">Left Coin</param>
    /// <param name="right">Right Coin</param>
    /// <returns>Left - Right</returns>
    public static IdleNum operator -(IdleNum left, IdleNum right) {
        IdleNum tmpIdleNum = new IdleNum(0, "");

        if (left.suffix == right.suffix) {
            // Same Suffix so we can just make a normal Subtraction
            tmpIdleNum.amount = left.amount - right.amount;

            // Take any of the 2 Suffixes cause they are the same anyway
            tmpIdleNum.suffix = left.suffix;
        } else {
            // We have to look wich side is Smaller and change the amount of that side to fit with the suffix of the other side

            // Convert the Suffixes to Integers so we can Compare them
            int leftSuffixInt = left.suffixToInt();
            int rightSuffixInt = right.suffixToInt();

            if (rightSuffixInt > leftSuffixInt) {
                // This Should NOT Happen because we would drop in the Negative range
                //Debug.LogError("Tried to Subtract too much (Right IdleNum suffix was Larger than Left IdleNum suffix) in - operator of Coin");
                return new IdleNum(0);
            } else if (leftSuffixInt > rightSuffixInt) {

                // Save the Difference between right and left
                int diff = leftSuffixInt - rightSuffixInt;

                // The right Suffix is Smaller than the left one
                if (diff > 4) {
                    // Double can only hold down to 15 numbers after ',' so just ignore it and return the way bigger Coin
                    return left;
                } else {
                    // Here everything with Diff 1-4 gets
                    // We calculate 1000 to the power of diff so we can move the number accordingly
                    double divider = System.Math.Pow(1000.0, diff);

                    // right is Smaller than right so we divide it and Subtract from left
                    // a Divider of 1000 ^ 2 would move the Number by 6 to the right
                    tmpIdleNum.amount = left.amount - (right.amount / divider);

                    // Take the Bigger Suffix
                    tmpIdleNum.suffix = left.suffix;
                }
            }
        }

        // When we substract the perfect amount we end up at zero
        // This will happen very rarely but almost allways happens when using the 
        // Lvlup Debug function
        // We would make an infinite loop at amount < 1
        if (tmpIdleNum.amount <= 0) {
            // So we just return 0
            return new IdleNum(0);
        }

        // If we Reached the lower than 1 with the last subtraction 
        // And while we are lower than that (For example when we remove almost the amount
        // Of coins we have we go near 0)
        // This is causing some inaccuracy wich will probably never effect the game
        // (if we make 10 coins/s and have 1 AZ + 1k and remove 1 AZ we get to 0 instead of 1 K
        // and will actually see the difference because 1 K is a lot with 10/s income)
        while (tmpIdleNum.amount < 1 && tmpIdleNum.suffix != "") {
            // We have to decrement the Suffix
            tmpIdleNum.decrementSuffix();
        }

        return tmpIdleNum;
    }

    /// <summary>
    /// Custom * Operator (IdleNum * float)
    /// </summary>
    /// <param name="left">Left Coin</param>
    /// <param name="right">Right Float</param>
    /// <returns>Left * Right</returns>
    public static IdleNum operator *(IdleNum left, double right) {
        IdleNum tmpIdleNum = new IdleNum(0, "");

        // Normal Mult
        tmpIdleNum.amount = left.amount * right;

        // When right = 0 or maybe other cases because of calculation errors?
        // We would make an infinite loop at amount < 1
        if(tmpIdleNum.amount == 0) {
            // So we just return 0
            return new IdleNum(0, "");
        }

        // Set Suffix to the only suffix available atm
        tmpIdleNum.suffix = left.suffix;

        // Because of Multiplication the amount can go over 1000 or below 1
        // If we Reached the 1000 Mark with the previous Multiplication
        while (tmpIdleNum.amount >= 1000) {
            // We have to increment the Suffix
            tmpIdleNum.incrementSuffix();
        }

        // If we Reached the lower than 1 with the last Multiplication 
        while (tmpIdleNum.amount < 1 && tmpIdleNum.suffix != "") { 
            // We have to decrement the Suffix
            tmpIdleNum.decrementSuffix();
        }

        return tmpIdleNum;
    }

    public static IdleNum operator *(IdleNum left, float right) {
        return left * Convert.ToDouble(right);
    }



    /// <summary>
    /// Custom / Operator (IdleNum / float)
    /// </summary>
    /// <param name="left">Left Coin</param>
    /// <param name="right">Right Float</param>
    /// <returns>Left / Right</returns>
    public static IdleNum operator /(IdleNum left, float right) {
        IdleNum tmpIdleNum = new IdleNum(0, "");

        // Normal Division
        tmpIdleNum.amount = left.amount / right;

        // Calculation errors?
        // We would make an infinite loop at amount < 1
        if (tmpIdleNum.amount == 0) {
            // So we just return 0
            return new IdleNum(0, "");
        }

        // Set Suffix to the only suffix available atm
        tmpIdleNum.suffix = left.suffix;

        // Because of Divison the amount can go over 1000 or below 1
        // If we Reached the 1000 Mark with the previous Division 
        while (tmpIdleNum.amount >= 1000) {
            // We have to increment the Suffix
            tmpIdleNum.incrementSuffix();
        }

        // If we Reached the lower than 1 with the last Multiplication 
        while (left.suffix != "" && tmpIdleNum.amount < 1) { 
            // We have to decrement the Suffix
            tmpIdleNum.decrementSuffix();
        }

        return tmpIdleNum;
    }

    /// <summary>
    /// Custom Equals Operator
    /// </summary>
    /// <param name="obj"></param>
    /// <returns></returns>
    public override bool Equals(object obj) {
        //Check for null and compare run-time types.

        if ((obj == null) || !this.GetType().Equals(obj.GetType())) {
            return false;
        } else {
            IdleNum othernum = (IdleNum)obj;
            return (this == othernum);
        }
    }

    /// <summary>
    /// Custom GetHashCode Operator
    /// </summary>
    /// <returns></returns>
    public override int GetHashCode() {
        return amount.GetHashCode() + suffix.GetHashCode();
    }


    /// <summary>
    /// Custom == Operator (IdleNum == IdleNum)
    /// </summary>
    /// <param name="left">Left Coin</param>
    /// <param name="right">Right Coin</param>
    /// <returns>Left == Right</returns>
    public static bool operator ==(IdleNum left, IdleNum right) {

        if (left.suffix == right.suffix) {
            // Normal Comparision between the Amounts
            return (left.amount == right.amount);
        } else {
            return false;
        }
    }

    /// <summary>
    /// Custom != Operator (IdleNum != IdleNum)
    /// </summary>
    /// <param name="left">Left Coin</param>
    /// <param name="right">Right Coin</param>
    /// <returns>Left != Right</returns>
    public static bool operator !=(IdleNum left, IdleNum right) {

        if (left.suffix == right.suffix) {
            // Normal Comparision between the Amounts
            return (left.amount != right.amount);
        } else {
            // Convert the Suffixes to Integers so we can Compare them
            return true;
        }
    }


    /// <summary>
    /// Custom >= Operator (IdleNum >= Coin)
    /// </summary>
    /// <param name="left">Left Coin</param>
    /// <param name="right">Right Coin</param>
    /// <returns>Left >= Right</returns>
    public static bool operator >=(IdleNum left, IdleNum right) {
        if (left.suffix == right.suffix) {
            return (left.amount >= right.amount);
        } else {
            // Compare the Suffixes instead with just > (because we already checked ==)

            // Convert the Suffixes to Integers so we can Compare them
            int leftSuffixInt = left.suffixToInt();
            int rightSuffixInt = right.suffixToInt();

            return (leftSuffixInt > rightSuffixInt);
        }
    }

    /// <summary>
    /// Custom > Operator (IdleNum > Coin)
    /// </summary>
    /// <param name="left">Left Coin</param>
    /// <param name="right">Right Coin</param>
    /// <returns>Left >= Right</returns>
    public static bool operator >(IdleNum left, IdleNum right) {
        if (left.suffix == right.suffix) {
            return (left.amount > right.amount);
        } else {
            // Compare the Suffixes instead with just > (because we already checked ==)

            // Convert the Suffixes to Integers so we can Compare them
            int leftSuffixInt = left.suffixToInt();
            int rightSuffixInt = right.suffixToInt();

            return (leftSuffixInt > rightSuffixInt);
        }
    }

    /// <summary>
    /// Custom <= Operator (IdleNum <= Coin)
    /// </summary>
    /// <param name="left">Left Coin</param>
    /// <param name="right">Right Coin</param>
    /// <returns>Left <= Right</returns>
    public static bool operator <=(IdleNum left, IdleNum right) {

        if(left.suffix == right.suffix) {
            // Normal Comparision between the Amounts
            return (left.amount <= right.amount);
        } else {
            // Compare the Suffixes instead with just < (because we already checked ==)

            // Convert the Suffixes to Integers so we can Compare them
            int leftSuffixInt = left.suffixToInt();
            int rightSuffixInt = right.suffixToInt();

            return (leftSuffixInt < rightSuffixInt);
        }
    }

    /// <summary>
    /// Custom < Operator (IdleNum <= Coin)
    /// </summary>
    /// <param name="left">Left Coin</param>
    /// <param name="right">Right Coin</param>
    /// <returns>Left <= Right</returns>
    public static bool operator <(IdleNum left, IdleNum right) {

        if (left.suffix == right.suffix) {
            // Normal Comparision between the Amounts
            return (left.amount < right.amount);
        } else {
            // Compare the Suffixes instead with just < (because we already checked ==)

            // Convert the Suffixes to Integers so we can Compare them
            int leftSuffixInt = left.suffixToInt();
            int rightSuffixInt = right.suffixToInt();

            return (leftSuffixInt < rightSuffixInt);
        }
    }
}
