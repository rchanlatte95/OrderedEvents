# OrderedEvents

Single-file library that allows you to order event function execution. Currently
at 190 LOC.

### Dependencies

None from what I can tell. 
Here are some caveats:

1. Compiled and tested for .NET 6, no implicit usings, targeting x86 Windows platform
2. Have not compiled for/with any .NET Framework version
3. Have not included or used any external packages or dependencies
4. Does not use any unsafe code blocks

### Quick Start

1. Download OrderedEvents.cs file
2. Place it into your solution/project
3. Compile and Build
4. Done

### Quick Example

```cs

TextBox OutputTxtBox = new();
Button TestOrderedEventBtn = new();
OrderedEvent<RoutedEventArgs> OrderedClick = new();

public void TestMethod0(object sender, RoutedEventArgs e) => OutputTxtBox.Text += $"0\n";
public void TestMethod1(object sender, RoutedEventArgs e) => OutputTxtBox.Text += $"1\n";
public void TestMethod2(object sender, RoutedEventArgs e) => OutputTxtBox.Text += $"2\n";
public void TestMethod3(object sender, RoutedEventArgs e) => OutputTxtBox.Text += $"3\n";
public void TestMethod4(object sender, RoutedEventArgs e) => OutputTxtBox.Text += $"4\n";
public void TestMethod5(object sender, RoutedEventArgs e) => OutputTxtBox.Text += $"5\n";
public void ResetTextBox(object sender, RoutedEventArgs e) => OutputTxtBox.Text = string.Empty;

public void Main()
{
	// Attach our OrderedEvent object to execute when the event is raised.
	TestOrderedEventBtn.Click += OrderedClick.Raise;
	
	// Subscribe some methods to executed when the button is pressed.
	// NOTE: Methods will generally execute at the order they were assigned.
	OrderedClick += TestMethod0;
	OrderedClick += TestMethod1;
	OrderedClick += TestMethod2;
	OrderedClick += TestMethod3;
	OrderedClick += TestMethod4;
	OrderedClick += TestMethod5;
	OrderedClick += ResetTextBox;
	
	// Supports removal
	OrderedClick -= TestMethod1;
	OrderedClick -= TestMethod2;
	
	// Decide I want the text box reset to be at the very start.
	var modifiedAct = OrderedClick.GetOrderedAction(ResetTextBox);
	if (modifiedAct != null) modifiedAct.ExecutionOrder = -100;
	
	// I'd also like the first method subscribed to execute at the very end instead.
	modifiedAct = OrderedClick.GetOrderedAction(TestMethod0);
	if (modifiedAct != null) modifiedAct.ExecutionOrder = 100;
	
	/*
	
	When TestOrderedEventBtn's Click event is raised, it will show the following
	output:
	
		3
		4
		5
		0
	*/
}

```

### License
MIT © [Ryan Chanlatte](https://github.com/rchanlatte95) 2023 