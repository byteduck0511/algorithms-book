// ==================== 应用一：括号匹配 ====================

static bool IsBalanced(string text)
{
    var stack = new Stack<char>();
    var pairs = new Dictionary<char, char> { [')'] = '(', [']'] = '[', ['}'] = '{' };

    foreach (char c in text)
    {
        if (c is '(' or '[' or '{')
        {
            stack.Push(c);                       // 遇到左括号，压栈
        }
        else if (pairs.TryGetValue(c, out char expectedOpen))
        {
            // 遇到右括号：栈顶必须是「配套的左括号」
            if (stack.Count == 0 || stack.Pop() != expectedOpen)
                return false;
        }
    }
    return stack.Count == 0;                     // 最后栈必须空了才算匹配
}

Console.WriteLine("=== 应用一：括号匹配 ===");
Console.WriteLine();

var cases = new[]
{
    "(1 + 2) * 3",
    "((1 + 2) * 3",
    "{[()]}",
    "{[(])}",
    "((()))",
    ")(",
    "a(b[c]d)e",
    "",
};

foreach (var text in cases)
{
    Console.WriteLine($"  {IsBalanced(text),-6} <- \"{text}\"");
}
Console.WriteLine();

Console.WriteLine("  关键点：遇到右括号时，栈顶必须是「最后打开的那个左括号」—— 这正是 LIFO。");
Console.WriteLine("  为什么不能只用一个计数器？因为 {[(])} 里括号数量是平衡的，但嵌套顺序是错的。");
Console.WriteLine();

// ==================== 应用二：中缀 → 后缀（调度场算法） ====================

static List<string> InfixToPostfix(string expr)
{
    var output = new List<string>();
    var ops = new Stack<char>();
    var precedence = new Dictionary<char, int> { ['+'] = 1, ['-'] = 1, ['*'] = 2, ['/'] = 2 };

    for (int i = 0; i < expr.Length; i++)
    {
        char c = expr[i];

        if (char.IsWhiteSpace(c)) continue;

        if (char.IsDigit(c))
        {
            // 读完整的一个数（支持多位数和小数点）
            int start = i;
            while (i < expr.Length && (char.IsDigit(expr[i]) || expr[i] == '.')) i++;
            output.Add(expr[start..i]);
            i--;                                  // 回退一格，外层 for 会再 i++
        }
        else if (c == '(')
        {
            ops.Push(c);
        }
        else if (c == ')')
        {
            while (ops.Count > 0 && ops.Peek() != '(')
                output.Add(ops.Pop().ToString());
            ops.Pop();                            // 把 '(' 弹掉，不输出
        }
        else if (precedence.ContainsKey(c))
        {
            // 栈顶运算符优先级 >= 当前运算符时，先输出栈顶（同级左结合）
            while (ops.Count > 0 && ops.Peek() != '(' &&
                   precedence[ops.Peek()] >= precedence[c])
                output.Add(ops.Pop().ToString());
            ops.Push(c);
        }
    }

    while (ops.Count > 0)
        output.Add(ops.Pop().ToString());

    return output;
}

// ==================== 应用三：后缀表达式求值 ====================

static double EvalPostfix(List<string> postfix)
{
    var stack = new Stack<double>();

    foreach (var token in postfix)
    {
        if (double.TryParse(token, out double num))
        {
            stack.Push(num);                      // 数字直接压栈
        }
        else
        {
            // 栈里不足两个数，说明表达式本身写错了（比如 "1 + + 2"）
            // 不加这个检查的话，会抛出 .NET 原始的 "Stack empty." —— 对使用者毫无意义
            if (stack.Count < 2)
                throw new InvalidOperationException($"运算符 '{token}' 缺少操作数，表达式格式有误");

            double b = stack.Pop();               // 注意：先弹出的是右操作数
            double a = stack.Pop();
            stack.Push(token switch
            {
                "+" => a + b,
                "-" => a - b,
                "*" => a * b,
                "/" => a / b,
                _ => throw new InvalidOperationException($"未知运算符: {token}")
            });
        }
    }
    return stack.Pop();
}

Console.WriteLine("=== 应用二 & 三：中缀 → 后缀 → 求值 ===");
Console.WriteLine();

var expressions = new[]
{
    "1 + 2 * 3",
    "(1 + 2) * 3",
    "10 - 4 / 2",
    "2 * (3 + 4) - 5",
    "((1 + 2) * (3 + 4)) / 7",
    "100 / 5 / 2",
};

Console.WriteLine($"  {"中缀表达式",-26} {"后缀表达式",-30} {"计算结果",10}");
Console.WriteLine(new string('-', 72));

foreach (var expr in expressions)
{
    var postfix = InfixToPostfix(expr);
    double value = EvalPostfix(postfix);
    Console.WriteLine($"  {expr,-26} {string.Join(" ", postfix),-30} {value,10}");
}
Console.WriteLine();

Console.WriteLine("  观察 100 / 5 / 2 的后缀形式：100 5 / 2 / —— 除号出现的顺序就是运算顺序。");
Console.WriteLine("  后缀表达式（逆波兰表示法）不需要括号，也不需要优先级规则，从头到尾扫一遍就能算。");
Console.WriteLine("  这就是为什么编译器内部的表达式处理，几乎都是先转成后缀再求值。");
Console.WriteLine();

// ==================== 应用四：完整的计算器（含错误处理） ====================

static bool TryEvaluate(string expr, out double result, out string error)
{
    result = 0;
    error = "";

    if (!IsBalanced(expr)) { error = "括号不匹配"; return false; }

    try
    {
        var postfix = InfixToPostfix(expr);
        if (postfix.Count == 0) { error = "表达式为空"; return false; }
        result = EvalPostfix(postfix);
        if (double.IsInfinity(result)) { error = "除以零"; return false; }
        return true;
    }
    catch (Exception ex)
    {
        error = ex.Message;
        return false;
    }
}

Console.WriteLine("=== 应用四：完整的计算器（含错误处理）===");
Console.WriteLine();

var inputs = new[]
{
    "1 + 2 * 3",
    "(1 + 2",
    "1 + + 2",
    "1 / 0",
    "3.5 * 2",
    "1 + 2) * 3",
};

foreach (var input in inputs)
{
    if (TryEvaluate(input, out double value, out string error))
        Console.WriteLine($"  ✓ {input,-16} = {value}");
    else
        Console.WriteLine($"  ✗ {input,-16} -> 错误: {error}");
}
Console.WriteLine();
