---
{ 
	"pageStylePath" : "Packages/com.passivepicasso.thunderkit/uss/thunderkit_style.uss",
	"title" : "DateTimeFormat Reference",
	"headerClasses" : [ "bm4", "page-header-container" ],
	"titleClasses" : [ "page-header" ],
	"iconUrl" : "Packages/com.passivepicasso.thunderkit/Documentation/graphics/TK_Documentation_2X_Icon.png",
	"iconClasses" : [ "header-icon" ]
}

---

Refer to [Custom Date and Time Format Strings](https://docs.microsoft.com/en-us/dotnet/standard/base-types/custom-date-and-time-format-strings) 
for more information about date and time formatting.


* "d"	The day of the month, from 1 through 31.
* "dd"	The day of the month, from 01 through 31.
* "ddd"	The abbreviated name of the day of the week.
* "dddd"	The full name of the day of the week.
* "f"	The tenths of a second in a date and time value.
* "ff"	The hundredths of a second in a date and time value.
* "fff"	The milliseconds in a date and time value.
* "ffff"	The ten thousandths of a second in a date and time value.
* "fffff"	The hundred thousandths of a second in a date and time value.
* "ffffff"	The millionths of a second in a date and time value.
* "fffffff"	The ten millionths of a second in a date and time value.
* "F"	If non-zero, the tenths of a second in a date and time value.
* "FF"	If non-zero, the hundredths of a second in a date and time value.
* "FFF"	If non-zero, the milliseconds in a date and time value.
* "FFFF"	If non-zero, the ten thousandths of a second in a date and time value.
* "FFFFF"	If non-zero, the hundred thousandths of a second in a date and time value.
* "FFFFFF"	If non-zero, the millionths of a second in a date and time value.
* "FFFFFFF"	If non-zero, the ten millionths of a second in a date and time value.
* "g", "gg"	The period or era.
* "h"	The hour, using a 12-hour clock from 1 to 12.
* "hh"	The hour, using a 12-hour clock from 01 to 12.
* "H"	The hour, using a 24-hour clock from 0 to 23.
* "HH"	The hour, using a 24-hour clock from 00 to 23.
* "K"	Time zone information.
* "m"	The minute, from 0 through 59.
* "mm"	The minute, from 00 through 59.
* "M"	The month, from 1 through 12.
* "MM"	The month, from 01 through 12.
* "MMM"	The abbreviated name of the month.
* "MMMM"	The full name of the month.
* "s"	The second, from 0 through 59.
* "ss"	The second, from 00 through 59.
* "t"	The first character of the AM/PM designator.
* "tt"	The AM/PM designator.
* "y"	The year, from 0 to 99.
* "yy"	The year, from 00 to 99.
* "yyy"	The year, with a minimum of three digits.
* "yyyy"	The year as a four-digit number.
* "yyyyy"	The year as a five-digit number.
* "z"	Hours offset from UTC, with no leading zeros.
* "zz"	Hours offset from UTC, with a leading zero for a single-digit value.
* "zzz"	Hours and minutes offset from UTC.
* ":"	The time separator.
* "/"	The date separator.
* "string" 'string'	Literal string delimiter.
* \	The escape character.

More information: Character literals and Using the Escape Character.	2009-06-15T13:45:30 (h \h) -> 1 h
Any other character	The character is copied to the result string unchanged.

More information: Character literals.	2009-06-15T01:45:30 (arr hh:mm t) -> arr 01:45 A