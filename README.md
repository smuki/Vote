**sunriseTpl** is a generating text output from source template and input parameters.
It can be used in many scenarios: website page building, email generation, source code generation, etc.
It's idea is based on antlr stringTemplate, but the syntax is based on c#,Unlike C#,the language is not case-sensitive.
This document is for version 0.01 of the engine.

Here is a very simple template:

- 1.
<pre class="CODE">
    Thank You for your order ${order.billFirstName} ${order.billLastName}.
</pre>
- 2.
<pre class="CODE">
	Your Order Total is: ${format(order.total, "C")}
</pre>
- 3.
<pre class="CODE">
    @if (test="${order.shipcountry != "US"}"){
        Your order will arrive in 2-3 weeks
    }@else{
        Your order will arrive in 5-7 days
    }~if
</pre>
- 4.
<pre class="CODE">
	@foreach (list="${list}" var="cust" index="i"){
	    ${i}: ${cust.lastname}, ${cust.firstname}
	}~foreach
</pre>


###The templates can have###

1. expressions.
2. if/elseif/else statement
3. foreach statement
4. for statement
5. Custom templates.

###Templates API:###

There are 2 classes mainly used in Template Engine:

1. Tmpl
2. TmplManager.

Template holds a single instance of a template and TmplManager is used for executing templates.

Easiest way of creating Templates is by using static methods of Template or TmplManager:

<pre class="CODE">
    Tmpl template = Tmpl.FromString(string name, string data)
	Tmpl template = Tmpl.FromFile(string name, string filename)
</pre>

then you use it to instantilate TmplManager.

<pre class="CODE">
    TmplManager mngr = new TmplManager(template);
</pre>

or even easier:

<pre class="CODE">
    TmplManager mngr = TmplManager.FromFile(filename);
    TmplManager mngr = TmplManager.FromString(template);
</pre>

when using FromString method, the string passed contains template code. This method can be used
to dynamically generate text without having templates in files.

You use setValue(string name, object value); to add values that can be used within the templates.

Ex:
<pre class="CODE">
	mngr.setValue("customer", new Customer("Tom", "Jackson"));
</pre>

then you can refer to customer within the template. You can use any type of object for value.
When the value of variable is to be output ToString() method will be called.

###Expressions###

Expressions are enclosed with ${ and } characters:

ex.
<pre class="CODE">
	${firstName}
</pre>

This example will output value of first name. If you need to output $ character, just escape it with another $$.

ex.

<pre class="CODE">
	Your SS $$ is ${ssnumber}
</pre>

Inside of expression block you can output any variable:

<pre class="CODE">
	${somevar}
</pre>

access property or field of a variable:

<pre class="CODE">
	${somestring.Lengt}
</pre>

property name is not case senstivie. So you can call: ${string.length} or

<pre class="CODE">
    ${string.LENGTH}
</pre>

or call a function:

<pre class="CODE">
	${trim(somename)}
</pre>

You can nest property accesses:
<pre class="CODE">
	${customer.firstname.length}
</pre>

You can also call methods on any objects:

<pre class="CODE">
	${firstname.substring(0, 5)}
</pre>

or
<pre class="CODE">
	${customer.isValid()}
</pre>

also allows you to use array access from indexed variables:

gets 3rd element of array

<pre class="CODE">
	${somearray[3]}
</pre>

gets value of "somekey" from hashtable.

<pre class="CODE">
	${hastable["somekey"]}
</pre>

You can use array access with any object that has indexer property.


Here is a list of the engine Operator/functions:

```
<table>
<tr><td> Operator/Function              <td> Description                                                                    <td> Example                               </tr>
<tr><td> --                             <td> :---                                                                           <td> :---                                  </tr>
<tr><td> +, -                           <td> Additive                                                                       <td> 100 + a                               </tr>
<tr><td> *, /                           <td> Multiplicative                                                                 <td> 100 * 2 / (3 % 2)                     </tr>
<tr><td> %                              <td> Mod                                                                            <td> 16 % 3                                </tr>
<tr><td> ^                              <td> Power                                                                          <td> 5.3 ^ 3                               </tr>
<tr><td> -                              <td> Negation                                                                       <td> -6 + 10                               </tr>
<tr><td> +                              <td> Concatenation                                                                  <td> "abc" + "def"                         </tr>
<tr><td> &                              <td> Concatenation                                                                  <td> "abc" & "def"                         </tr>
<tr><td> ==, !=, <, >, <=, >=           <td> Comparison                                                                     <td> 2.5 > 100                             </tr>
<tr><td> And, Or                        <td> Logical                                                                        <td> (1 > 10) and (true or not false)      </tr>
<tr><td> not(boolvalue)                 <td> Logical                                                                        <td> not(true or not false)                </tr>
<tr><td> IIf                            <td> Conditional                                                                    <td> IIf(a > 100, "greater", "less")       </tr>
<tr><td> .                              <td> Member                                                                         <td> varA.varB.function("a")               </tr>
<tr><td> String                         <td> literal                                                                        <td> "string!"                             </tr>
<tr><td> number                         <td> double integer                                                                 <td> 100+97.21                             </tr>
<tr><td> Boolean                        <td> literal                                                                        <td> true AND false                        </tr>
<tr><td> isnull(object)                 <td> test whether object is null                                                    <td> isnull(var)                           </tr>
<tr><td> isnullorempty(string)          <td> test whether string isnullorempty.                                             <td> isnullorempty(var)                    </tr>
<tr><td> isnotempty(string)             <td> test whether string has at least 1 character.                                  <td> isnotempty(var)                       </tr>
<tr><td> toupper(string)                <td> converts string to upper case                                                  <td> toupper(var)                          </tr>
<tr><td> tolower(string)                <td> converts string to lower case                                                  <td> tolower(var)                          </tr>
<tr><td> trim(string)                   <td> will trim string object                                                        <td> trim(var)                             </tr>
<tr><td> len(string)                    <td> returns length of string                                                       <td> len(var)                              </tr>
<tr><td> cint(value)                    <td> converts value to integer                                                      <td> cint(var)                             </tr>
<tr><td> cdouble(value)                 <td> converts value to double                                                       <td> cdouble(var)                          </tr>
<tr><td> cdate(value)                   <td> converts value to datetime                                                     <td> cdate(var)                            </tr>
<tr><td> isnumber(num)                  <td> tests whether num is of numeric type                                           <td> isnumber(var)                         </tr>
<tr><td> isdefined(varname)             <td> tests whether variable varname is defined                                      <td> isdefined(varname)                    </tr>
<tr><td> ifdefined(varname,varname)     <td> returns value if varname is defined.otherwise return nothing                   <td> ifdefined("varname",varname)          </tr>
<tr><td> typeof(object)                 <td> return string representation of the type of object.                            <td> typeof(object)                        </tr>
<tr><td> format(object,formatstring)    <td> will call Tostring(formatstring) on object                                     <td> format(object)                        </tr>
<tr><td> replace(string,f1,r1)          <td> return search f1 and replaces with r1                                          <td> replace("a apple","apple","orange")   </tr>
<tr><td> round(value,decimal,opt)       <td> rounds a number to the given number of decimal places                          <td> round(3.456,2)                        </tr>
<tr><td> indexof(value,find)            <td> returns the first occurrence of a letter in a string.                          <td> indexof("one,two,tree","two")         </tr>
<tr><td> join(list,property,separator)  <td> return elements of an array separated by the specified separator               <td> join(list,",")/join(list,"name",","   </tr>
<tr><td> split(string,sep1,sep2)        <td> Split string into array by separator                                           <td> split("one,two,tree,four",",")        </tr>
<tr><td> sweep(string,s1,s2,s3)         <td> sweep string (s1,s2...)                                                        <td> sweep("o,t,x,f","t","f")              </tr>
<tr><td> filter(mylist,booleanproperty) <td> return new list from mylist for those objects whose property evaluates to true <td> filter(mylist,"display")              </tr>
<tr><td> typeref(type)                  <td> create a reference type                                                        <td> TypeRef("System.Math").Round(3.39789) </tr>
</table>


###Statement:###

**IF**

You can also conditionally output text based on some expression using special if tag:

<pre class="CODE">
    @if (test="${booleanexpression}"){

    }@elseif (test="${bool}"){

    }@else{

    }~if
</pre>

elseif and else are optional. If condition of "if" evaluates to true, then block inside of "if" will be output, otherwise
elseif Condition will be evaluates (if exists) and then else.

Ex:

<pre class="CODE">
@if (test="${cust.country == "US"}"){
    You are US customer.
}@else{
    You are from: ${cust.country} country.
}~if
</pre>
If cust.country is "US" then the output will be: You are US customer.

**FOREACH**

You can loop through list of elements (any object that implements IEnumerable interface) using FOREACH tag.
<pre class="CODE">
@foreach (list="${list}" var="cust" index="i"){
    ${i}: ${cust.lastname}, ${cust.firstname}
}~foreach
</pre>

Suppose customers is array of customer objects: customers = Customer("Tom", "Jackson"), Customer("Mary", "Foo")

The output will be:

1. Jackson, Tom
2. Foo, Mary

During execution, variable name that is passed as var attribute will be assigned with element from the list.
Index attribute can be omitted, and is used to represent index variable for the loop. It starts with 1 and gets
increments with each iteration.

**FOR**

You can use FOR tab to loop through integer values by one.
<pre class="CODE">
@for (from="1" to="10" index="i"){
    ${i}: ${customers[i].name}
}~for
</pre>

**Custom Templates:**

You can also create your own templates inside of template file that you can call.
You do that using define statement:
<pre class="CODE">

@define (name="customer_template"){
    ${customer.lastname}, ${customer.firstname}
}~define

@using tmpl="customer_template" customer="${cust}" ~

</pre>
You can pass any attributes to the template, and you can use those inside of the template.
The template can also access all variables that are defined outside of the template.
When calling template you have to put trailing slash at the end, or put closing tag:

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

You can also nest those:
<pre class="CODE">
@define (name="italic"){${innerText}}~define

@using (tmpl="bold"){@using (tmpl="italic"){${cust.lastname}, ${cust.firstname}}~using}~using
</pre>

Templates can be nested inside other template:
<pre class="CODE">
@define (name="doit"){
	@define (name="colorme"){
	  font color=${color}${innerText}
	}~define

    @using (tmpl="colorme" color="blue"){colorize me}~using
}~define
</pre>
colorme template can only be used within doit template.

Templates can also be added programmatically:

<pre class="CODE">
TmplManager mngr = ...;
mngr.AddTemplate(Template.FromString("bold", "bef---${innerText}---aft"));
</pre>

now bold template can be used anywhere within processing.

END.
