<xsl:stylesheet
    xmlns:xs="http://www.w3.org/2001/XMLSchema"
    xmlns:xsl="http://www.w3.org/1999/XSL/Transform" version="2.0"
    exclude-result-prefixes="xs">

  <xsl:output method="xml" encoding="UTF-8" indent="yes" />

  <xsl:template match="Survey">
    <xsl:if test="count(//SurveyItem/Response[@Type eq 'Date']) gt 0">
      <xsl:element name="SurveyCode">
        <xsl:for-each select="SurveyItem[Response/@Type eq 'Date']">
          <xsl:variable name="precedingNodes" select="preceding-sibling::node()" />
          <xsl:variable name="precedingSurveyItems" select="$precedingNodes[compare(name(), 'SurveyItem') eq 0]" />
          <xsl:variable name="itemNum" select="count($precedingSurveyItems/Response[compare(@Type, 'Instruction') ne 0])" />
          <xsl:variable name="liNum" select="count($precedingSurveyItems)" />
          <xsl:element name="ItemFunctions">
            <xsl:attribute name="ItemNum" select="$itemNum" />
          <xsl:call-template name="BuildDateFunctions">
            <xsl:with-param name="itemNum" select="$itemNum" />
            <xsl:with-param name="liNum" select="$liNum" />
          </xsl:call-template>
          </xsl:element>
        </xsl:for-each>
      </xsl:element>
    </xsl:if>
  </xsl:template>

  <xsl:template name="BuildDateFunctions" >
    <xsl:param name="itemNum" as="xs:integer" />
    <xsl:param name="liNum" as="xs:integer" />
    <xsl:element name="Function">
      <xsl:attribute name="FunctionName">
        <xsl:value-of select="concat('InitializeItem', $itemNum)" />
      </xsl:attribute>
      <xsl:attribute name="FunctionType" select="'initialization'" />
      <xsl:element name="Params" />
      <xsl:element name="FunctionBody">
        <xsl:element name="Code">initFunctions.push(function  {</xsl:element>
        <xsl:element name="Code">
          <xsl:value-of select="concat('answerInput = document.getElementById(&quot;Item', $itemNum, '&quot;);')" />
        </xsl:element>
        <xsl:element name="Code">answerInput.value = "";</xsl:element>
      </xsl:element>
    </xsl:element>
    <xsl:element name="Function">
      <xsl:attribute name="FunctionName">
        <xsl:value-of select="concat('ValidateInput', $itemNum)"/>
      </xsl:attribute>
      <xsl:attribute name="FunctionType" select="'validation'" />
      <xsl:element name="Params" />
      <xsl:element name="FunctionBody">
        <xsl:element name="Code">var daysInMonths = [31, 29, 31, 30, 31, 30, 31, 31, 30, 31, 30, 31];</xsl:element>
        <xsl:element name="Code">
          <xsl:value-of select="concat('questionLI = document.getElementById(&quot;ItemLITag', $liNum, '&quot;);')" />
        </xsl:element>
        <xsl:element name="Code">
          <xsl:value-of select="concat('answerInput = document.getElementById(&quot;Item', $itemNum, ');')" />
        </xsl:element>
        <xsl:element name="Code">var answer = answerInput.value;</xsl:element>
        <xsl:element name="Code">var error = 0;</xsl:element>
        <xsl:element name="Code">var dateExp = /^[1-9][0-2]?\/([1-3][0-9]|[1-9])\/[0-9]{4}$/;</xsl:element>
        <xsl:element name="Code">if (!dateExp.test(answer))</xsl:element>
        <xsl:element name="Code">error = 1;</xsl:element>
        <xsl:element name="Code">if (error == 0) {</xsl:element>
        <xsl:element name="Code">var vals = answer.split("/");</xsl:element>
        <xsl:element name="Code">var month;</xsl:element>
        <xsl:element name="Code">var month = parseInt(vals[0], 10);</xsl:element>
        <xsl:element name="Code">day = parseInt(vals[1], 10);</xsl:element>
        <xsl:element name="Code">var year = parseInt(vals[2], 10);</xsl:element>
        <xsl:element name="Code">if (isNaN(month))</xsl:element>
        <xsl:element name="Code">error = 2;</xsl:element>
        <xsl:element name="Code">else if (isNaN(day))</xsl:element>
        <xsl:element name="Code">error = 3;</xsl:element>
        <xsl:element name="Code">else if (isNaN(year))</xsl:element>
        <xsl:element name="Code">error = 4;</xsl:element>
        <xsl:element name="Code">else if ((month &#x3C; 1) || (month &#x3E; 12))</xsl:element>
        <xsl:element name="Code">error = 2;</xsl:element>
        <xsl:element name="Code">else if ((day &#x3C; 1) || (day &#x3E; daysInMonths[month - 1]))</xsl:element>
        <xsl:element name="Code">error = 3;</xsl:element>
        <xsl:element name="Code">else if ((day == 29) &#x26;&#x26; (month == 2) &#x26;&#x26; (year % 4))</xsl:element>
        <xsl:element name="Code">error = 3;</xsl:element>
        <xsl:element name="Code">else if (year &#x3C; 0)</xsl:element>
        <xsl:element name="Code">error = 4;</xsl:element>
        <xsl:if test="./StartDate[@HasValue eq 'True']" >
          <xsl:element name="Code">
            <xsl:value-of select="concat('if (year &#x3C; ', ./StartDate/Year, ')')" />
          </xsl:element>
          <xsl:element name="Code">error = 5;</xsl:element>
          <xsl:element name="Code">
            <xsl:value-of select="concat('else if (year == ', StartDate/Year, ') {')" />
          </xsl:element>
          <xsl:element name="Code">
            <xsl:value-of select="concat('if (month &#x3C; ', StartDate/Month, ')')"/>
          </xsl:element>
          <xsl:element name="Code">error = 5;</xsl:element>
          <xsl:element name="Code">
            <xsl:value-of select="concat('else if (month == ', StartDate/Month, ')')"/>
          </xsl:element>
          <xsl:element name="Code">
            <xsl:value-of select="concat('if (day &#x3C; ', StartDate/Day, ')')"/>
          </xsl:element>
          <xsl:element name="Code">error = 5;</xsl:element>
          <xsl:element name="Code">}</xsl:element>
        </xsl:if>
        <xsl:if test="./EndDate[@HasValue eq 'True']" >
          <xsl:element name="Code">
            <xsl:value-of select="concat('if (year &#x3E; ', EndDate/Year, ')')"/>
          </xsl:element>
          <xsl:element name="Code">error = 6;</xsl:element>
          <xsl:element name="Code">
            <xsl:value-of select="concat('else if (year == ', EndDate/Year, ') {')" />
          </xsl:element>
          <xsl:element name="Code">
            <xsl:value-of select="concat('if (month &#x3E; ', EndDate/Month, ')')" />
          </xsl:element>
          <xsl:element name="Code">error = 6;</xsl:element>
          <xsl:element name="Code">
            <xsl:value-of select="concat('else if (month == ', EndDate/Month, ')')" />
          </xsl:element>
          <xsl:element name="Code">
            <xsl:value-of select="concat('if (day &#x3E; ', EndDate/Day, ')')"/>
          </xsl:element>
          <xsl:element name="Code">error = 6;</xsl:element>
          <xsl:element name="Code">}</xsl:element>
        </xsl:if>
        <xsl:element name="Code">}</xsl:element>
        <xsl:element name="Code">
          var monthNames = ["January", "February", "March", "April", "May", "June", "July", "August", "September", "October", "November", "December"];
        </xsl:element>
        <xsl:element name="Code">if ((error != 0) &#x26;&#x26; (!ForceSubmit)) {</xsl:element>
        <xsl:element name="Code">var errorMsgLI = document.createElement("li");</xsl:element>
        <xsl:element name="Code">errorMsgLI.className = "Error";</xsl:element>
        <xsl:element name="Code">var errorMsg;</xsl:element>
        <xsl:element name="Code">if (error == 1)</xsl:element>
        <xsl:element name="Code">errorMsg = document.createTextNode("Please enter a date in MM/DD/YYYY format for the question below.");</xsl:element>
        <xsl:element name="Code">else if (error == 2)</xsl:element>
        <xsl:element name="Code">errorMsg = document.createTextNode("The supplied month is not valid.");</xsl:element>
        <xsl:element name="Code">else if (error == 3)</xsl:element>
        <xsl:element name="Code">errorMsg = document.createTextNode("The supplied day is not valid.");</xsl:element>
        <xsl:element name="Code">else if (error == 4)</xsl:element>
        <xsl:element name="Code">errorMsg = document.createTextNode("The supplied year is not valid.");</xsl:element>
        <xsl:element name="Code">else if (error == 5)</xsl:element>
        <xsl:element name="Code">
          <xsl:variable name="date" select="concat('monthNames[', StartDate/Month, ' - 1] + &quot; ', StartDate/Day, ', ', StartDate/Year, '&quot;')" />
          <xsl:value-of select="concat('errorMsg = document.createTextNode(&quot;Please enter a date that falls on or after &quot; + ', $date, ');')" />
        </xsl:element>
        <xsl:element name="Code">else if (error == 6)</xsl:element>
        <xsl:element name="Code">
          <xsl:variable name="date" select="concat('monthNames[', EndDate/Month, ' - 1] + &quot; ', EndDate/Day, ', ', EndDate/Year, '&quot;')" />
          <xsl:value-of select="concat('errorMsg = document.createTextNode(&quot;Please enter a date that falls on or before &quot; + ', $date, ');')" />
        </xsl:element>
        <xsl:element name="Code">errorMsgLI.appendChild(errorMsg);</xsl:element>
        <xsl:element name="Code">questionListNode.insertBefore(errorMsgLI, questionLI);</xsl:element>
        <xsl:element name="Code">return 1;</xsl:element>
        <xsl:element name="Code">} else if (error != 0) {</xsl:element>
        <xsl:element name="Code">answerInput.value = "NULL";</xsl:element>
        <xsl:element name="Code">return 1;</xsl:element>
        <xsl:element name="Code">}</xsl:element>
        <xsl:element name="Code">return 0;</xsl:element>
      </xsl:element>
    </xsl:element>
  </xsl:template>
</xsl:stylesheet>