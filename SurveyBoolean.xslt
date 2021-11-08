<xsl:stylesheet
    xmlns:xs="http://www.w3.org/2001/XMLSchema"
    xmlns:xsl="http://www.w3.org/1999/XSL/Transform" version="2.0"
    exclude-result-prefixes="xs">

  <xsl:output method="xml" encoding="UTF-8" indent="yes" />

  <xsl:template match="Survey">
    <xsl:if test="count(//SurveyItem/Response[@Type eq 'Boolean']) gt 0">
      <xsl:element name="SurveyCode">
        <xsl:for-each select="SurveyItem[Response/@Type eq 'Boolean']">
          <xsl:variable name="precedingNodes" select="preceding-sibling::node()" />
          <xsl:variable name="precedingSurveyItems" select="$precedingNodes[compare(name(), 'SurveyItem') eq 0]" />
          <xsl:variable name="itemNum" select="count($precedingSurveyItems/Response[compare(@Type, 'Instruction') ne 0])" />
          <xsl:variable name="liNum" select="count($precedingSurveyItems)" />
          <xsl:element name="ItemFunctions">
            <xsl:attribute name="ItemNum" select="$itemNum" />
          <xsl:call-template name="BuildBooleanFunctions">
            <xsl:with-param name="itemNum" select="$itemNum" />
            <xsl:with-param name="liNum" select="$liNum" />
          </xsl:call-template>
          </xsl:element>
        </xsl:for-each>
      </xsl:element>
    </xsl:if>
  </xsl:template>

  <xsl:template name="BuildBooleanFunctions" >
    <xsl:param name="itemNum" />
    <xsl:param name="liNum" />
    <xsl:element name="Function">
      <xsl:attribute name="FunctionName">
        <xsl:value-of select="concat('InitializeItem', $itemNum)" />
      </xsl:attribute>
      <xsl:attribute name="FunctionType" select="'initialization'" />
      <xsl:element name="Params" />
      <xsl:element name="FunctionBody">
        <xsl:element name="Code">
          <xsl:value-of select="concat('var answerInputs = document.getElementsByName(&quot;Item', $itemNum, '&quot;);')" />
        </xsl:element>
        <xsl:element name="Code">for (var ctr = 0; ctr &#x3C; answerInputs.length; ctr++)</xsl:element>
        <xsl:element name="Code">answerInputs[ctr].checked = false;</xsl:element>
      </xsl:element>
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
        <xsl:element name="Code">
          <xsl:value-of select="concat('var answerInputs = document.getElementsByName(&quot;Item', $itemNum, '&quot;);')" />
        </xsl:element>
        <xsl:element name="Code">var ctr = 0;</xsl:element>
        <xsl:element name="Code">var selectionMade = false;</xsl:element>
        <xsl:element name="Code">for (ctr = 0; ctr &#x3C; answerInputs.length; ctr++)</xsl:element>
        <xsl:element name="Code">if (answerInputs[ctr].checked)</xsl:element>
        <xsl:element name="Code">selectionMade = true;</xsl:element>
        <xsl:element name="Code">if (!selectionMade) {</xsl:element>
        <xsl:element name="Code">if (!ForceSubmit) {</xsl:element>
        <xsl:element name="Code">var errorMsgLI = document.createElement("li");</xsl:element>
        <xsl:element name="Code">errorMsgLI.className = "Error";</xsl:element>
        <xsl:element name="Code">var errorMsg = document.createTextNode("Please select a response to the question below.");</xsl:element>
        <xsl:element name="Code">errorMsgLI.appendChild(errorMsg);</xsl:element>
        <xsl:element name="Code">questionListNode.insertBefore(errorMsgLI, questionLI);</xsl:element>
        <xsl:element name="Code">} else {</xsl:element>
        <xsl:element name="Code">answerInputs[0].checked = true;</xsl:element>
        <xsl:element name="Code">answerInputs[0].value = "NULL";</xsl:element>
        <xsl:element name="Code">}</xsl:element>
        <xsl:element name="Code">return 1;</xsl:element>
        <xsl:element name="Code">}</xsl:element>
        <xsl:element name="Code">return 0;</xsl:element>
      </xsl:element>
    </xsl:element>
  </xsl:template>
</xsl:stylesheet>