/*
TargetDate = "03/22/2010 12:00 AM";
BackColor = "";
ForeColor = "";
CountActive = true;
CountStepper = -1;
LeadingZero = true;
DisplayFormat = "<li class='first'>%%D%%</li> <li>%%H%%</li> <li>%%M%%</li> <li>%%S%%</li>";
FinishMessage = "Done";

function calcage(secs, num1, num2) {
		s = ((Math.floor(secs/num1))%num2).toString();
	if (LeadingZero && s.length < 2)
		s = "0" + s;
	return "<em>" + s + "</em>";
}

function CountBack(secs) {
	if (secs < 0) {
		document.getElementById("cntdwn").innerHTML = FinishMessage;
		return;
	}
	DisplayStr = DisplayFormat.replace(/%%D%%/g, calcage(secs,86400,100000));
	DisplayStr = DisplayStr.replace(/%%H%%/g, calcage(secs,3600,24));
	DisplayStr = DisplayStr.replace(/%%M%%/g, calcage(secs,60,60));
	DisplayStr = DisplayStr.replace(/%%S%%/g, calcage(secs,1,60));
	
	document.getElementById("cntdwn").innerHTML = DisplayStr;
	if (CountActive)
		setTimeout("CountBack(" + (secs+CountStepper) + ")", SetTimeOutPeriod);
}

function putspan(backcolor, forecolor) {
	document.write("<div id='widget'><ul id='cntdwn'></ul><a id='link' href='http://www.microsoft.com/visualstudio/en-us/products/msdn/default.mspx#roadmap' target='_blank'></a></div>");
}

CountStepper = Math.ceil(CountStepper);
if (CountStepper == 0)
	CountActive = false;
var SetTimeOutPeriod = (Math.abs(CountStepper)-1)*1000 + 990;
putspan(BackColor, ForeColor);
var dthen = new Date(TargetDate);
var dnow = new Date();
if(CountStepper>0)
	ddiff = new Date(dnow-dthen);
else
	ddiff = new Date(dthen-dnow);
gsecs = Math.floor(ddiff.valueOf()/1000);
CountBack(gsecs);
*/

