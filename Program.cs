using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CalculateAlgebraicExpressions
{
    class Program
    {
        static string ConditionalArguments = "";
        static char[] ValidCharacters = { '1', '2', '3', '4', '5', '6', '7', '8', '9', '0', '+', '-', '*', '/', '(', ')', '.' };
        static char[] Numerics = { '1', '2', '3', '4', '5', '6', '7', '8', '9', '0', '.' };
        static char[] Operators = { '+', '-', '*', '/', '(', ')' };
        static char[] MDOperators = { '*', '/' };//multiplication,division
        static char[] ASOperators = { '+', '-' };//addtion,subtraction
        static void Main(string[] args)
        {
            Console.WriteLine("请输入只含有 +、-、*、/、(、)和数字 的代数表达式，我将计算其结果。");
            while (true)
            {
                string Expression = Console.ReadLine().Replace('）', ')').Replace('（', '(').Replace('\\', '/').Replace(" ", "");//这里对空格、中文括号和反斜杠进行处理                
                int indexOfValid = IsValid(Expression);
                if (indexOfValid != -1)
                {
                    Console.WriteLine("意外的字符 " + Expression[indexOfValid] + " 位于第" + (indexOfValid + 1).ToString() + "个字符的位置！");
                    Console.WriteLine(ConditionalArguments);
                }
                else
                {
                    Expression = OptimizeExpression(Expression);//对式子进行优化
                    Console.WriteLine(Expression + " 的计算结果是\n" + Calculation(Expression));
                }
                Console.WriteLine();
            }
        }
        static int IsValid(string e)//如果字符串不合法，会返回其不合法字符的索引,如果合法，返回-1
        {
            //检测是否右括号先于左括号出现用的标记，当左括号出现时做标记，将该bool值变为true            
            bool rightParentheseShouldApear = false;
            //检测是否右括号多于左括号用
            int numbersOfLeftParentheses = 0;
            int numbersOfRightParentheses = 0;
            //检测操作符+-*/()之后是否出现了*和/之用，同时检测第一个字符是否是*或/，检测)之前是否是操作符
            char LastChar = '*';
            foreach (char c in e)
            {
                if (!ValidCharacters.Contains(c))//检测是否有无意外的字符
                {
                    ConditionalArguments = "不应该出现字符 " + c;
                    return e.IndexOf(c);
                }
                else if (c == '(')//检测是否右括号先于左括号出现
                {
                    rightParentheseShouldApear = true;
                    numbersOfLeftParentheses += 1;
                }
                else if (c == ')')
                {
                    numbersOfRightParentheses += 1;
                    if (rightParentheseShouldApear == false)
                    {
                        ConditionalArguments = "第一个右括号没有对应的左括号匹配！";
                        return e.IndexOf(c);

                    }
                    if (Operators.Contains(LastChar) && LastChar != ')')
                    {
                        ConditionalArguments = "无意义的操作符 " + LastChar;
                        return e.IndexOf(c) - 1;
                    }
                }
                else if (Operators.Contains(LastChar) && (c == '*' || c == '/') && LastChar != ')')
                {
                    ConditionalArguments = "无意义的操作符 " + c;
                    return e.IndexOf(c);
                }
                else if (e.Contains("()"))
                {
                    ConditionalArguments = "无意义的空括号";
                    return e.IndexOf("()");
                }
                else if (Numerics.Contains(c) && LastChar == ')')
                {
                    ConditionalArguments = "右括号与数字之间不应该省略乘号！";
                    return e.IndexOf(")" + c) + 1;
                }
                LastChar = c;
            }
            if (numbersOfRightParentheses > numbersOfLeftParentheses)
            {
                ConditionalArguments = "右括号多于左括号！";
                return e.IndexOf(')');
            }
            else if (Operators.Contains(e[e.Count() - 1]) && e[e.Count() - 1] != ')')
            {
                ConditionalArguments = "操作符位于代数式末尾";
                return e.Count() - 1;
            }
            else
            {
                bool startCheck = false;
                int count = 0;
                foreach (char c in e)
                {
                    count += 1;
                    if (!startCheck)
                    {
                        if (c == '.') startCheck = true;
                    }
                    else
                    {
                        if (Operators.Contains(c))
                            startCheck = false;
                        else if (c == '.')
                        {
                            ConditionalArguments = "一个数字不应该有两个小数点！";
                            return count - 1;
                        }
                    }
                }
            }
            return -1;
        }

        
        static string OptimizeExpression(string e)
        {
            //在(8)(9)中间补出乘号
            e = e.Replace(")(", ")*(");
            //在数字与左括号之间加上乘号
            foreach (char c in Numerics)
            {
                string oldOne = c + "(", newOne = c + "*(";
                e = e.Replace(oldOne, newOne);
            }
            //优化小数点
            e = e.Replace(".*", ".0*").Replace("./", ".0/").Replace(".+", ".0+").Replace(".-", ".0-").Replace(".)", ".0)").
                Replace("*.", "*0.").Replace("/.", "/0.").Replace("+.", "+0.").Replace("-.", "-0.").Replace("(.", "(0.");
            //如果右括号多于左括号，则补齐
            int numbersOfLeftParentheses = 0;
            int numbersOfRightParentheses = 0;
            foreach (char c in e)
                if (c == '(')
                    numbersOfLeftParentheses += 1;
                else if (c == ')')
                    numbersOfRightParentheses += 1;
            if (numbersOfLeftParentheses > numbersOfRightParentheses)
                e = e.PadRight(e.Count() + numbersOfLeftParentheses - numbersOfRightParentheses, ')');
            return e;
        }
        static string Calculation(string e)
        {
            //先简化++--
            while (e.Contains("++") || e.Contains("+-") || e.Contains("-+") || e.Contains("--") || e.Contains("*+") || e.Contains("/+") || e.Contains("(+"))
                e = e.Replace("++", "+").Replace("+-", "-").Replace("-+", "-").Replace("--", "+").Replace("*+", "*").Replace("/+", "/").Replace("(+", "(");
            //再对括号进行迭代算法
            while (e.Contains('('))//如果有括号的话
            {
                int ParentheseValue = 1;//数字为1表示有1个左括号等待右括号匹配，数字为x则表示有x个左括号等待匹配
                for (int i = e.IndexOf('(') + 1; i <= e.Count() - 1; i++)
                {
                    char c = e[i];
                    if (c == '(') ParentheseValue += 1;
                    else if (c == ')') ParentheseValue -= 1;
                    if (ParentheseValue == 0)
                    {
                        string newE = e.Substring(e.IndexOf('(') + 1, i - e.IndexOf('(') - 1);//newE是括号内的式子
                        //核心算法
                        e = e.Replace('(' + newE + ')', Calculation(newE));//对括号内的式子进行迭代计算
                        break;
                    }
                }
            }
            //运行到这里表示式子没有括号了
            //先乘除
            while (e.Contains('*') || e.Contains('/'))
            {
                //检测乘除号作用的范围，比如1-+5*6/-3-7作用的范围是5*6/-3                
                int index = -1;
                if (e.Contains('*') && !e.Contains('/'))
                    index = e.IndexOf('*') - 1;
                else if (!e.Contains('*') && e.Contains('/'))
                    index = e.IndexOf('/') - 1;
                else
                    index = Math.Min(e.IndexOf('*'), e.IndexOf('/')) - 1;
                int leftIndex = -1;
                int rightIndex = -1;
                while (true)//向左检测并记录index
                {
                    if (index < 0)
                    {
                        leftIndex = 0;
                        break;
                    }
                    else if (!Numerics.Contains(e[index]))
                    {
                        leftIndex = index + 1;
                        break;
                    }
                    index -= 1;
                }
                //做标记表示是否允许单操作符'-'
                index = -1;
                if (e.Contains('*') && !e.Contains('/'))
                    index = e.IndexOf('*') + 1;
                else if (!e.Contains('*') && e.Contains('/'))
                    index = e.IndexOf('/') + 1;
                else
                    index = Math.Min(e.IndexOf('*'), e.IndexOf('/')) + 1;
                bool SubtractionAllowed = true;
                while (true)//向右检测
                {
                    if (index >= e.Count())
                    {
                        rightIndex = e.Count() - 1;
                        break;
                    }
                    else if (e[index] == '-')
                        if (SubtractionAllowed == true)
                            SubtractionAllowed = false;
                        else
                        {
                            rightIndex = index - 1;
                            break;
                        }
                    else if (MDOperators.Contains(e[index]))
                        SubtractionAllowed = true;
                    else if (!Numerics.Contains(e[index]))
                    {
                        rightIndex = index - 1;
                        break;
                    }
                    index += 1;
                }
                string newE = e.Substring(leftIndex, rightIndex - leftIndex + 1);
                e = e.Replace(newE, MDCalculation(newE));
            }
            //后加减
            //再次简化++--
            while (e.Contains("++") || e.Contains("+-") || e.Contains("-+") || e.Contains("--") || e.Contains("(+"))
                e = e.Replace("++", "+").Replace("+-", "-").Replace("-+", "-").Replace("--", "+").Replace("(+", "(");
            //去掉index=0的正号
            if (e[0] == '+') e = e.Substring(1);
            while (e.Contains('+') || e.Substring(1).Contains('-'))
            {
                //从左至右
                char Operator = 'n';
                string a = "", b = "";
                int i = 0;
                if (e[0] == '-')
                {
                    a += '-';
                    i = 1;
                }
                for (; !ASOperators.Contains(e[i]); i++)
                    a += e[i];
                Operator = e[i];
                i += 1;
                for (; i < e.Count() && !MDOperators.Contains(e[i]); i++)
                    b += e[i];
                int index = i - 1;
                string newE = "";
                if (Operator == '+')
                    newE = (Convert.ToDouble(a) + Convert.ToDouble(b)).ToString();
                else
                    newE = (Convert.ToDouble(a) - Convert.ToDouble(b)).ToString();
                e = e.Replace(a + Operator + b, newE);
            }


            return e;
        }
        static string MDCalculation(string e)
        {
            //从左到右计算
            while (e.Contains('*') || e.Contains('/'))
            {
                char Operator = 'n';
                string a = "", b = "";
                int i = 0;
                for (; !MDOperators.Contains(e[i]); i++)
                    a += e[i];
                Operator = e[i];
                i += 1;
                for (; i < e.Count() && !MDOperators.Contains(e[i]); i++)
                    b += e[i];
                int index = i - 1;
                string newE = "";
                if (Operator == '*')
                    newE = (Convert.ToDouble(a) * Convert.ToDouble(b)).ToString();
                else
                    newE = (Convert.ToDouble(a) / Convert.ToDouble(b)).ToString();
                e = e.Replace(a + Operator + b, newE);
            }
            return e;
        }
    }

}
