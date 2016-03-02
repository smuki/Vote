** T ** 是一种模板根据输入的参数输出文本的一个引擎。
它可以在很多的使用环境如根据模板产生网页, 根据模板产生邮件, 根据模板产生代码等等.
它的语法类似于C#,不区分大小写.
这个文档是为T引擎 版本 1.0而写。

下列是简单的模板.

- 1.
<pre class="CODE">
    谢谢你的邮件$order.billFirstName ${order.billLastName}.
</pre>
- 2.
<pre class="CODE">
	金额合计: ${format(order.total, "C")}
</pre>
- 3.
<pre class="CODE">
    @if (test="${order.shipcountry != "HK"}"){
       你的快件将会在1-2个星期到达。
}@elseif (test="${order.shipcountry != "HK"}"){
    }@else{
       你的快件将会在2-6天到达。
    }~if
</pre>
- 4.
<pre class="CODE">
	@foreach (list="${list}" var="cust" index="i"){
	    ${i}: ${cust.lastname}, ${cust.firstname}
	}~foreach
</pre>


###模板可以包含如下元素###

1. 表达式.
2. if/elseif/else 语句
3. foreach 语句
4. for 语句
5. 用户自定义模板.

###模板 API:###

下面是引擎两个主要的类:

1. Tmpl
2. TmplManager.


下面是 Tmpl or TmplManager 的简单用法:

<pre class="CODE">
    Tmpl template = Tmpl.LoadString(string name, string data)
	Tmpl template = Tmpl.LoadFile(string name, string filename)
</pre>

 TmplManager的使用方法.

<pre class="CODE">
    TmplManager mngr = new TmplManager(template);
</pre>

或更简单:

<pre class="CODE">
    TmplManager mngr = TmplManager.LoadFile(filename);
    TmplManager mngr = TmplManager.LoadString(template);
</pre>

当你使用 LoadString ,可以直接使用字符串的内容为模板不必使用模板文件

使用 SetValue(string name, object value); 之后可以在模板内使用这不变量.

例如:
<pre class="CODE">
	mngr.SetValue("customer", new Customer("Tom", "Jackson"));
</pre>

然后你可以在模板中引用 customer. 你可以使用任何类型的变量.
输出变量时会调用 ToString().

###表达式###

表达式用${ 开头 } 结尾:

例如.
<pre class="CODE">
	${firstName}
</pre>

这个例子讲输出firstname的内容. 如果你想输出字符 $ ,只需使用双$,如 $$.

例如.
<pre class="CODE">
	你的 $$ is ${ssnumber}
</pre>

在表达式快内，你可以输出任何变量:

<pre class="CODE">
	${somevar}
</pre>

输出变量的属性:

<pre class="CODE">
	${somestring.Lengt}
</pre>

变量属性名是不区分大小写的.所以你可以: ${string.length} 或 ${string.LENGTH}

<pre class="CODE">
    ${string.LENGTH}
</pre>

或调用function:

<pre class="CODE">
	${trim(somename)}
</pre>

你可以如此使用属性:
<pre class="CODE">
	${customer.firstname.length}
</pre>

你也可以调用任何objects的方法.

<pre class="CODE">
	${firstname.substring(0, 5)}
</pre>

或者
<pre class="CODE">
	${customer.isValid()}
</pre>

你也可以使用index存取array.

读取array第3个元素

<pre class="CODE">
	${somearray[3]}
</pre>

在hastable中读取 "somekey" 的值.

<pre class="CODE">
	${hastable["somekey"]}
</pre>

下表是Engine的操作符和Function:

<table>
<thead>
<tr>
  <th>操作符/Function</th>
  <th style="text-align:left;">描述</th>
  <th style="text-align:left;">例子</th>
</tr>
</thead>
<tr>
  <td>+, -</td>
  <td style="text-align:left;">加减法</td>
  <td style="text-align:left;">100 + a</td>
</tr>
<tr>
  <td>*, /</td>
  <td style="text-align:left;">乘除法</td>
  <td style="text-align:left;">100 * 2 / (3 % 2)</td>
</tr>
<tr>
  <td>%</td>
  <td style="text-align:left;">Mod</td>
  <td style="text-align:left;">16 % 3</td>
</tr>
<tr>
  <td>^</td>
  <td style="text-align:left;">Power</td>
  <td style="text-align:left;">5.3 ^ 3</td>
</tr>
<tr>
  <td>-</td>
  <td style="text-align:left;">负数</td>
  <td style="text-align:left;">-6 + 10</td>
</tr>
<tr>
  <td>+</td>
  <td style="text-align:left;">合并</td>
  <td style="text-align:left;">&#8220;abc&#8221; + &#8220;def&#8221;</td>
</tr>
<tr>
  <td>&amp;</td>
  <td style="text-align:left;">合并</td>
  <td style="text-align:left;">&#8220;abc&#8221; &amp; &#8220;def&#8221;</td>
</tr>
<tr>
  <td>==, !=, &lt;, >, &lt;=, >=</td>
  <td style="text-align:left;">比较</td>
  <td style="text-align:left;">2.5 > 100</td>
</tr>
<tr>
  <td>And, Or</td>
  <td style="text-align:left;">逻辑</td>
  <td style="text-align:left;">(1 > 10) and (true or not false)</td>
</tr>
<tr>
  <td>not(boolvalue)</td>
  <td style="text-align:left;">逻辑</td>
  <td style="text-align:left;">not(true or not false)</td>
</tr>
<tr>
  <td>IIf</td>
  <td style="text-align:left;">条件</td>
  <td style="text-align:left;">IIf(a > 100, &#8220;greater&#8221;, &#8220;less&#8221;)</td>
</tr>
<tr>
  <td>.</td>
  <td style="text-align:left;">成员</td>
  <td style="text-align:left;">varA.varB.function(&#8221;a&#8221;)</td>
</tr>
<tr>
  <td>String</td>
  <td style="text-align:left;">文字</td>
  <td style="text-align:left;">&#8220;string!&#8221;</td>
</tr>
<tr>
  <td>number</td>
  <td style="text-align:left;">数字</td>
  <td style="text-align:left;">100+97.21</td>
</tr>
<tr>
  <td>Boolean</td>
  <td style="text-align:left;">逻辑类型</td>
  <td style="text-align:left;">true AND false</td>
</tr>
<tr>
  <td>isnull(object)</td>
  <td style="text-align:left;">检测object是否为null</td>
  <td style="text-align:left;">isnull(var)</td>
</tr>
<tr>
  <td>isnullorempty(string)</td>
  <td style="text-align:left;">检测 string 是否为null或空</td>
  <td style="text-align:left;">isnullorempty(var)</td>
</tr>
<tr>
  <td>isnotempty(string)</td>
  <td style="text-align:left;">检测string是否为空</td>
  <td style="text-align:left;">isnotempty(var)</td>
</tr>
<tr>
  <td>toupper(string)</td>
  <td style="text-align:left;">将string转为大写字母</td>
  <td style="text-align:left;">toupper(var)</td>
</tr>
<tr>
  <td>tolower(string)</td>
  <td style="text-align:left;">将string转为小写字母</td>
  <td style="text-align:left;">tolower(var)</td>
</tr>
<tr>
  <td>trim(string)</td>
  <td style="text-align:left;">删除string后的空格</td>
  <td style="text-align:left;">trim(var)</td>
</tr>
<tr>
  <td>len(string)</td>
  <td style="text-align:left;">返回string的长度</td>
  <td style="text-align:left;">len(var)</td>
</tr>
<tr>
  <td>cint(value)</td>
  <td style="text-align:left;">将value 转为 integer</td>
  <td style="text-align:left;">cint(var)</td>
</tr>
<tr>
  <td>cdouble(value)</td>
  <td style="text-align:left;">将value 转为 double</td>
  <td style="text-align:left;">cdouble(var)</td>
</tr>
<tr>
  <td>cdate(value)</td>
  <td style="text-align:left;">将value 转为 datetime</td>
  <td style="text-align:left;">cdate(var)</td>
</tr>
<tr>
  <td>isnumber(num)</td>
  <td style="text-align:left;">检测num是否为数字</td>
  <td style="text-align:left;">isnumber(var)</td>
</tr>
<tr>
  <td>isdefined(varname)</td>
  <td style="text-align:left;">检测varname 是否已经定义</td>
  <td style="text-align:left;">isdefined(varname)</td>
</tr>
<tr>
  <td>ifdefined(varname,varname)</td>
  <td style="text-align:left;">如果varname 已经定义，这返回varname的值，否则范围空白</td>
  <td style="text-align:left;">ifdefined(&#8221;varname&#8221;,varname)</td>
</tr>
<tr>
  <td>typeof(object)</td>
  <td style="text-align:left;">返回object 的类型</td>
  <td style="text-align:left;">typeof(object)</td>
</tr>
<tr>
  <td>format(object,formatstring)</td>
  <td style="text-align:left;">调用object的 Tostring(formatstring) 方法</td>
  <td style="text-align:left;">format(object)</td>
</tr>
<tr>
  <td>replace(string,f1,r1)</td>
  <td style="text-align:left;">将string中的f1替换为r1</td>
  <td style="text-align:left;">replace(&#8221;a apple&#8221;,&#8221;apple&#8221;,&#8221;orange&#8221;)</td>
</tr>
<tr>
  <td>round(value,decimal,opt)</td>
  <td style="text-align:left;">对value做4舍5入，小数位为decimal</td>
  <td style="text-align:left;">round(3.456,2)</td>
</tr>
<tr>
  <td>indexof(value,find)</td>
  <td style="text-align:left;">查找find 在value第一次出现的位置</td>
  <td style="text-align:left;">indexof(&#8221;one,two,tree&#8221;,&#8221;two&#8221;)</td>
</tr>
<tr>
  <td>join(list,property,separator)</td>
  <td style="text-align:left;">用separator将array的元素property合并为一个字符串</td>
  <td style="text-align:left;">join(list,&#8221;,&#8221;)/join(list,&#8221;name&#8221;,&#8221;,&#8221;</td>
</tr>
<tr>
  <td>split(string,sep1,sep2)</td>
  <td style="text-align:left;">根据sep1,sep2将string分拆为一个array</td>
  <td style="text-align:left;">split(&#8221;one,two,tree,four&#8221;,&#8221;,&#8221;)</td>
</tr>
<tr>
  <td>sweep(string,s1,s2,s3)</td>
  <td style="text-align:left;">剔除string中包含的s1,s2,s3</td>
  <td style="text-align:left;">sweep(&#8221;o,t,x,f&#8221;,&#8221;t&#8221;,&#8221;f&#8221;)</td>
</tr>
<tr>
  <td>filter(mylist,booleanproperty)</td>
  <td style="text-align:left;">返回一个所有booleanproperty为true的列表</td>
  <td style="text-align:left;">filter(mylist,&#8221;display&#8221;)</td>
</tr>
<tr>
  <td>typeref(type)</td>
  <td style="text-align:left;">创建一个reference 类型</td>
  <td style="text-align:left;">TypeRef(&#8221;System.Math&#8221;).Round(3.39789)</td>
</tr>
</table>

| 操作符/Function                   | 描述                                  | 例子                                    |
| --                             | :---                                | :---                                  |
| +, -                           | 加减法                                 | 100 + a                               |
| *, /                           | 乘除法                                 | 100 * 2 / (3 % 2)                     |
| %                              | Mod                                 | 16 % 3                                |
| ^                              | Power                               | 5.3 ^ 3                               |
| -                              | 负数                                  | -6 + 10                               |
| +                              | 合并                                  | "abc" + "def"                         |
| &                              | 合并                                  | "abc" & "def"                         |
| ==, !=, <, >, <=, >=           | 比较                                  | 2.5 > 100                             |
| And, Or                        | 逻辑                                  | (1 > 10) and (true or not false)      |
| not(boolvalue)                 | 逻辑                                  | not(true or not false)                |
| IIf                            | 条件                                  | IIf(a > 100, "greater", "less")       |
| .                              | 成员                                  | varA.varB.function("a")               |
| String                         | 文字                                  | "string!"                             |
| number                         | 数字                                  | 100+97.21                             |
| Boolean                        | 逻辑类型                                | true AND false                        |
| isnull(object)                 | 检测object是否为null                     | isnull(var)                           |
| isnullorempty(string)          | 检测 string 是否为null或空                 | isnullorempty(var)                    |
| isnotempty(string)             | 检测string是否为空                        | isnotempty(var)                       |
| toupper(string)                | 将string转为大写字母                       | toupper(var)                          |
| tolower(string)                | 将string转为小写字母                       | tolower(var)                          |
| trim(string)                   | 删除string后的空格                        | trim(var)                             |
| len(string)                    | 返回string的长度                         | len(var)                              |
| cint(value)                    | 将value 转为 integer                   | cint(var)                             |
| cdouble(value)                 | 将value 转为 double                    | cdouble(var)                          |
| cdate(value)                   | 将value 转为 datetime                  | cdate(var)                            |
| isnumber(num)                  | 检测num是否为数字                          | isnumber(var)                         |
| isdefined(varname)             | 检测varname 是否已经定义                    | isdefined(varname)                    |
| ifdefined(varname,varname)     | 如果varname 已经定义，这返回varname的值，否则范围空白  | ifdefined("varname",varname)          |
| typeof(object)                 | 返回object 的类型                        | typeof(object)                        |
| format(object,formatstring)    | 调用object的 Tostring(formatstring) 方法 | format(object)                        |
| replace(string,f1,r1)          | 将string中的f1替换为r1                    | replace("a apple","apple","orange")   |
| round(value,decimal,opt)       | 对value做4舍5入，小数位为decimal             | round(3.456,2)                        |
| indexof(value,find)            | 查找find 在value第一次出现的位置               | indexof("one,two,tree","two")         |
| join(list,property,separator)  | 用separator将array的元素property合并为一个字符串 | join(list,",")/join(list,"name",","   |
| split(string,sep1,sep2)        | 根据sep1,sep2将string分拆为一个array        | split("one,two,tree,four",",")        |
| sweep(string,s1,s2,s3)         | 剔除string中包含的s1,s2,s3                | sweep("o,t,x,f","t","f")              |
| filter(mylist,booleanproperty) | 返回一个所有booleanproperty为true的列表       | filter(mylist,"display")              |
| typeref(type)                  | 创建一个reference 类型                    | TypeRef("System.Math").Round(3.39789) |


###语句:###

**IF**

你可以根据表达式条件输出文本:

<pre class="CODE">
    @if (test="${booleanexpression}"){

    }@elseif (test="${bool}"){

    }@else{

    }~if
</pre>

elseif 和 else 是可选的 . 如果 test 的计算结果为 true, if 下的快将输出.

例子:

<pre class="CODE">
@if (test="${cust.country == "HK"}"){
    你是来自香港的客户.
}@else{
    你来自:${cust.country}.
}~if
</pre>
如果 cust.country 是 "HK" 就输出: 你是来自香港的客户.

**FOREACH**

你可以使用FOREACH遍历列表的所有元素.
<pre class="CODE">
@foreach (list="${list}" var="cust" index="i"){
    ${i}: ${cust.lastname}, ${cust.firstname}
}~foreach
</pre>

假设 list 是一个客户列表 : list = Customer("Tony", "Jackson"), Customer("Mary", "Foo")

输出结果是:

1. Jackson, Tony
2. Foo, Mary

执行期间,list的元素将会赋值给变量,index属性是可选的，

**FOR**

你可以使用For 循环 从一个整数到另外一个整数
<pre class="CODE">
@for (from="1" to="10" index="i"){
    ${i}: ${customers[i].name}
}~for
</pre>

**Custom Templates:**

你可以在模板内定义模板.
例如:
<pre class="CODE">

@define (name="customer_template"){
    ${customer.lastname}, ${customer.firstname}
}~define

@using tmpl="customer_template" customer="${cust}" ~

</pre>
你可以传送任何属性给模板, 然后再模板中使用.
模板也可以存取所有模板外定义的变量.

<pre class="CODE">
@using (tmpl="you_onw_template_name") ~

</pre>

The template also received special variable: innerText that is the content of executing the inner elements of calling template.
<pre class="CODE">
@define (name="bold"){
${innerText}
}~define

@using (tmpl="bold"){${cust.lastname}, ${cust.firstname}}~using
</pre>
the output will be:
Jackson, Tom
(if customer is Tom Jackson)

你也可以这样写
<pre class="CODE">
@define (name="italic"){${innerText}}~define

@using (tmpl="bold"){@using (tmpl="italic"){${cust.lastname}, ${cust.firstname}}~using}~using
</pre>

模板可以定义在另外一个模板内:
<pre class="CODE">
@define (name="doit"){
	@define (name="colorme"){
	  font color=${color}${innerText}
	}~define

    @using (tmpl="colorme" color="blue"){colorize me}~using
}~define
</pre>
colorme 模板之可以在doit 模板内使用.

你也可以使用程序加上模板

<pre class="CODE">
TmplManager mngr = ...;
mngr.AddTemplate(Template.LoadString("bold", "bef---${innerText}---aft"));
</pre>

现在 bold 模板可以在这个TmplManager中的任何地方使用了.
