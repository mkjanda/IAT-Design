<xsl:stylesheet
    xmlns:xs="http://www.w3.org/2001/XMLSchema"
    xmlns:xsl="http://www.w3.org/1999/XSL/Transform" version="2.0"
    exclude-result-prefixes="xs">

  <xsl:output method="xml" encoding="UTF-8" indent="yes" />

  <xsl:template match="Survey">
    <xsl:if test="count(//SurveyItem/Response[@Type eq 'Multiple Choice']) gt 0">
      <xsl:element name="SurveyCode">
        <xsl:for-each select="SurveyItem[Response/@Type eq 'Multiple Choice']">
          <xsl:variable name="precedingNodes" select="preceding-sibling::node()" />
          <xsl:variable name="precedingSurveyItems" select="$precedingNodes[compare(name(), 'SurveyItem') eq 0]" />
          <xsl:variable name="itemNum" select="count($precedingSurveyItems/Response[compare(@Type, 'Instruction') ne 0])" />
          <xsl:variable name="liNum" select="count($precedingSurveyItems)" />
          <xsl:element name="ItemFunctions">
            <xsl:attribute name="ItemNum" select="$itemNum" />
          <xsl:call-template name="BuildMultipleSelectionFunctions">
            <xsl:with-param name="itemNum" select="$itemNum" />
            <xsl:with-param name="liNum" select="$liNum" />
          </xsl:call-template>
          </xsl:element>
        </xsl:for-each>
      </xsl:element>
    </xsl:if>
  </xsl:template>

  <xsl:template name="BuildMultipleSelectionFunctions" >
    <xsl:param name="itemNum" as="xs:integer" />
    <xsl:param name="liNum" as="xs:integer" />
    <xsl:element name="Function">
      <xsl:attribute name="FunctionName">
        <xsl:value-of select="concat('InitializeItem', $itemNum)" />
      </xsl:attribute>
      <xsl:attribute name="FunctionType" select="'initialization'" />
      <xsl:element name="Params" />
      <xsl:element name="FunctionBody" />
    </xsl:element>
    <xsl:element name="Function">
      <xsl:attribute name="FunctionName">
        <xsl:value-of select="concat('ValidateItem', $itemNum)" />
      </xsl:attribute>
      <xsl:attribute name="FunctionType" select="'validation'" />
      <xsl:element name="Params" />
      <xsl:element name="FunctionBody">
        <xsl:element name="Code">
          <xsl:value-of select="concat('var questionLI = document.getElementById(&quot;ItemLITag', $liNum, '&quot;);')" />
        </xsl:element>
        <xsl:element name="Code">var ctr = 0;</xsl:element>
        <xsl:element name="Code">var nChecked = 0;</xsl:element>
        <xsl:element name="Code">
          <xsl:value-of select="concat('for (ctr = 0; ctr &#x3E; ', count(./Labels/Label), '; ctr++) {')" />
        </xsl:element>
        <xsl:element name="Code">
          <xsl:value-of select="concat('var selectionInput = document.getElementById(&quot;Item', $itemNum, '_&quot;.concat((ctr + 1).toString()));')" />
        </xsl:element>
        <xsl:element name="Code">if (selectionInput.checked)</xsl:element>
        <xsl:element name="Code">nChecked++;</xsl:element>
        <xsl:element name="Code">}</xsl:element>
        <xsl:element name="Code">var errorMsgLI, errorMsg;</xsl:element>
        <xsl:element name="Code">
          <xsl:value-of select="concat('if (nChecked &#x3C; ', ./MinSelections, ') {')"/>
        </xsl:element>
        <xsl:element name="Code">if (!ForcedSubmit) {</xsl:element>
        <xsl:element name="Code">errorMsgLI = document.createElement("li");</xsl:element>
        <xsl:element name="Code">errorMsgLI.className = "Error";</xsl:element>
        <xsl:if test="./MinSelections eq '1'" >
          <xsl:element name="Code">
            <xsl:value-of select="concat('errorMsg = document.createTextNode(&quot;Please select at least ', ./MinSelections, '&quot; response to the question below.&quot;);')" />
          </xsl:element>
        </xsl:if>
        <xsl:if test="./MinSelections ne '1'" >
          <xsl:element name="Code">
            <xsl:value-of select="concat('errorMsg = document.createTextNode(&quot;Please select at least ', ./MinSelections, ' responses to the question below.&quot;);')" />
          </xsl:element>
        </xsl:if>
        <xsl:element name="Code">errorMsgLI.appendChild(errorMsg);</xsl:element>
        <xsl:element name="Code">questionListNode.insertBefore(errorMsgLI, questionLI);</xsl:element>
        <xsl:element name="Code">} else {</xsl:element>
        <xsl:element name="Code">var responseElement = document.createElement("input");</xsl:element>
        <xsl:element name="Code">
          <xsl:value-of select="concat('responseElement.name = &quot;Item', $itemNum, '&quot;;')" />
        </xsl:element>
        <xsl:element name="Code">responseElement.type = "hidden";</xsl:element>
        <xsl:element name="Code">responseElement.value = "NULL";</xsl:element>
        <xsl:element name="Code">}</xsl:element>
        <xsl:element name="Code">return 1;</xsl:element>
        <xsl:element name="Code">
          <xsl:value-of select="concat('} else if (nChecked &#x3E; ', ./MaxSelections, ') {')" />
        </xsl:element>
        <xsl:element name="Code">if (!ForceSubmit) {</xsl:element>
        <xsl:element name="Code">errorMsgLI = document.createElement("li");</xsl:element>
        <xsl:element name="Code">errorMsgLI.className = "Error";</xsl:element>
        <xsl:if test="./MaxSelections eq '1'" >
          <xsl:element name="Code">
            <xsl:value-of select="concat('errorMsg = document.createTextNode(&quot;Please select no more than ', ./MaxSelections, ' response to the question below.&quot;);')" />
          </xsl:element>
        </xsl:if>
        <xsl:if test="./MaxSelections ne '1'" >
          <xsl:element name="Code">
            <xsl:value-of select="concat('errorMsg = document.createTextNode(&quot;Please select no more than ', ./MaxSelections, ' responses to the question below.&quot;);')" />
          </xsl:element>
        </xsl:if>
        <xsl:element name="Code">errorMsgLI.appendChild(errorMsg);</xsl:element>
        <xsl:element name="Code">questionListNode.insertBefore(errorMsgLI, questionLI);</xsl:element>
        <xsl:element name="Code">} else {</xsl:element>
        <xsl:element name="Code">var responseElement = document.createElement("input");</xsl:element>
        <xsl:element name="Code">
          <xsl:value-of select="concat('responseElement.name = &quot;Item', $itemNum, '&quot;;')" />
        </xsl:element>
        <xsl:element name="Code">responseElement.type = "hidden";</xsl:element>
        <xsl:element name="Code">responseElement.value = "NULL";</xsl:element>
        <xsl:element name="Code">}</xsl:element>
        <xsl:element name="Code">return 1;</xsl:element>
        <xsl:element name="Code">}</xsl:element>
        <xsl:element name="Code">var responseElement = document.createElement("input");</xsl:element>
        <xsl:element name="Code">
          <xsl:value-of select="concat('responseElement.name = &quot;Item', $itemNum, '&quot;;')" />
        </xsl:element>
        <xsl:element name="Code">responseElement.type = "hidden";</xsl:element>
        <xsl:element name="Code">responseElement.value = "";</xsl:element>
        <xsl:element name="Code">
          <xsl:value-of select="concat('for (ctr = 0; ctr &#x3C; ', count(./Labels/Label), '; ctr++) {')" />
        </xsl:element>
        <xsl:element name="Code">
          <xsl:value-of select="concat('selectionInput = document.getElementById(&quot;Item', $itemNum, '_&quot;.concat((ctr + 1).toString()));')" />
        </xsl:element>
        <xsl:element name="Code">if (selectionInput.checked)</xsl:element>
        <xsl:element name="Code">responseElement.value += "1";</xsl:element>
        <xsl:element name="Code">else</xsl:element>
        <xsl:element name="Code">responseElement.value += "0";</xsl:element>
        <xsl:element name="Code">}</xsl:element>
        <xsl:element name="Code">
          <xsl:value-of select="concat('responseElement.id = &quot;MultiSelectItem', $itemNum, '&quot;;')" />
        </xsl:element>
        <xsl:element name="Code">var dupElement = document.getElementById(responseElement.id);</xsl:element>
        <xsl:element name="Code">if (dupElement != null)</xsl:element>
        <xsl:element name="Code">dupElement.value = responseElement.value;</xsl:element>
        <xsl:element name="Code">else</xsl:element>
        <xsl:element name="Code">questionListNode.appendChild(responseElement);</xsl:element>
        <xsl:element name="Code">return 0;</xsl:element>
      </xsl:element>
    </xsl:element>
  </xsl:template>
</xsl:stylesheet>