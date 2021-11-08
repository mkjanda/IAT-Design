<xsl:stylesheet
    xmlns:xs="http://www.w3.org/2001/XMLSchema"
    xmlns:xsl="http://www.w3.org/1999/XSL/Transform" version="2.0"
    exclude-result-prefixes="xs">

  <xsl:output method="xml" encoding="UTF-8" indent="yes" />

  <xsl:variable name="Functions">
    <xsl:element name="Function">
      <xsl:attribute name="FunctionName" select="'isNumber'" />
      <xsl:attribute name="FunctionType" select="'global'" />
      <xsl:element name="Params">
        <xsl:element name="Param">n</xsl:element>
      </xsl:element>
      <xsl:element name="FunctionBody">
        <xsl:element name="Code">var exp = /^[1-9]?[0-9]*?0?.?[0-9]*$/;</xsl:element>
        <xsl:element name="Code">return exp.test(n);</xsl:element>
      </xsl:element>
    </xsl:element>

    <xsl:element name="Function">
      <xsl:attribute name="FunctionName" select="'validateSurvey'" />
      <xsl:attribute name="FunctionType" select="'global'" />
      <xsl:element name="Params" />
      <xsl:element name="FunctionBody">
        <xsl:element name="Code">var ctr = 0;</xsl:element>
        <xsl:element name="Code">while (ctr &#x3C; questionListNode.childNodes.length) {</xsl:element>
        <xsl:element name="Code">if (questionListNode.childNodes[ctr].className == "Error")</xsl:element>
        <xsl:element name="Code">questionListNode.removeChild(questionListNode.childNodes[ctr]);</xsl:element>
        <xsl:element name="Code">else</xsl:element>
        <xsl:element name="Code">ctr++;</xsl:element>
        <xsl:element name="Code">}</xsl:element>
        <xsl:element name="Code">var nErrors = 0;</xsl:element>
        <xsl:element name="Code">for (ctr = 0; ctr &#x3C; validateFunctions.length; ctr++) {</xsl:element>
        <xsl:element name="Code">var itemErrors = validateFunctions[ctr].call()</xsl:element>
        <xsl:element name="Code">if (itemErrors != 0)</xsl:element>
        <xsl:element name="Code">initFunctions[ctr].call();</xsl:element>
        <xsl:element name="Code">nErrors += itemErrors</xsl:element>
        <xsl:element name="Code">}</xsl:element>
        <xsl:element name="Code">return nErrors;</xsl:element>
      </xsl:element>
    </xsl:element>

    <xsl:element name="Function">
      <xsl:attribute name="FunctionName" select="'OnSubmit'" />
      <xsl:attribute name="FunctionType" select="'global'" />
      <xsl:element name="Params">
        <xsl:element name="Param">event</xsl:element>
      </xsl:element>
      <xsl:element name="FunctionBody">
        <xsl:element name="Code">if (ForceSubmit == true)</xsl:element>
        <xsl:element name="Code">return;</xsl:element>
        <xsl:element name="Code">var nErrors = validateSurvey();</xsl:element>
        <xsl:element name="Code">if (nErrors &#x3E; 0) {</xsl:element>
        <xsl:element name="Code">event = EventUtil.getEvent(event);</xsl:element>
        <xsl:element name="Code">if (ErrorsExistDiv == null) {</xsl:element>
        <xsl:element name="Code">ErrorsExistDiv = document.getElementById("ErrorsExistMsgDiv");</xsl:element>
        <xsl:element name="Code">var ErrorsExistMsg = document.createElement("h3");</xsl:element>
        <xsl:element name="Code">var ErrorsExistMsgText = document.createTextNode("Response errors detected. Please review the above survey for error messages and then resubmit.");</xsl:element>
        <xsl:element name="Code">ErrorsExistMsg.appendChild(ErrorsExistMsgText);</xsl:element>
        <xsl:element name="Code">ErrorsExistDiv.appendChild(ErrorsExistMsg);</xsl:element>
        <xsl:element name="Code">}</xsl:element>
        <xsl:element name="Code">EventUtil.preventDefault(event);</xsl:element>
        <xsl:element name="Code">} else {</xsl:element>
        <xsl:element name="Code">var submitButton = document.getElementById("SubmitButton");</xsl:element>
        <xsl:element name="Code">submitButton.disabled = true;</xsl:element>
        <xsl:element name="Code">}</xsl:element>
      </xsl:element>
    </xsl:element>

    <xsl:element name="Function">
      <xsl:attribute name="FunctionName" select="'DoForceSubmit'" />
      <xsl:attribute name="FunctionType" select="'global'" />
      <xsl:element name="Params" />
      <xsl:element name="FunctionBody">
        <xsl:element name="Code">ForceSubmit = true;</xsl:element>
        <xsl:element name="Code">validateSurvey();</xsl:element>
        <xsl:element name="Code">document.getElementById("SurveyForm").submit();</xsl:element>
      </xsl:element>
    </xsl:element>
  </xsl:variable>

  <xsl:variable name="VariableDeclarations">
    <Declarations>
      <Declaration>var ForceSubmit = false;</Declaration>
      <Declaration>var questionListNode = document.getElementById("QuestionList");</Declaration>
      <Declaration>var initFunctions = new Array();</Declaration>
      <Declaration>var validateFunctions = new Array();</Declaration>
      <Declaration>var ErrorsExistDiv = null;</Declaration>
      <Declaration>var form = document.getElementById("SurveyForm");</Declaration>
    </Declarations>
  </xsl:variable>

  <xsl:variable name="GlobalAbbreviations">
    <xsl:variable name="Globals" select="string-join(for $elem in $variableDeclarations/Declarations/Declaration return replace($elem, '^var\s+(.+);$', '$1'), ', ')" />
    <xsl:analyze-string select="$Globals" regex="([A-Za-z_][A-Za-z0-9_]*)(\s*(=((\s+|[^;=/,&#34;\(]+?|&#34;[^&#34;\n\r]*?&#34;|\(([^;=,&#34;]*?,?(&#34;[^\n\r&#34;]*?&#34;)?)+\)|/[^/\n]+?/)*)+?)?)">
      <xsl:matching-substring>
        <xsl:element name="Entry">
          <xsl:attribute name="type" select="'global'" />
          <xsl:element name="OrigName">
            <xsl:value-of select="regex-group(1)" />
          </xsl:element>
          <xsl:element name="NewName">
            <xsl:value-of select="concat('_g', position())" />
          </xsl:element>
          <xsl:element name="Assign">
            <xsl:value-of select="regex-group(4)" />
          </xsl:element>
        </xsl:element>
      </xsl:matching-substring>
    </xsl:analyze-string>
  </xsl:variable>

  <xsl:variable name="GlobalCode">
    <xsl:element name="Code">EventUtil.addHandler(form, "submit", OnSubmit);</xsl:element>
    <xsl:if test="@TimeoutMillis ne '0'">
      <xsl:element name="Code">
        <xsl:value-of select="concat('setTimeout(DoForceSubmit, ', @TimeoutMillis, ');')"/>
      </xsl:element>
    </xsl:if>
    <xsl:for-each select="for $i in 1 to count(//SurveyItem) return $i">
      <xsl:element name="Code">
        <xsl:value-of select="concat('initFunctions.push(InitializeItem', ., ');')" />
      </xsl:element>
      <xsl:element name="Code">
        <xsl:value-of select="concat('validateFunctions.push(ValidateItem', ., ');')" />
      </xsl:element>
    </xsl:for-each>
  </xsl:variable>

  <xsl:template match="Survey">
    <xsl:element name="CodeFile">
      <xsl:element name="VarEntries">
        <xsl:copy-of select="$GlobalAbbreviations"/>
      </xsl:element>
      <xsl:element name="Functions">
        <xsl:copy-of select="$Functions" />
      </xsl:element>
      <xsl:element name="GlobalCode">
        <xsl:copy-of select="$GlobalCode" />
      </xsl:element>
    </xsl:element>
  </xsl:template>
</xsl:stylesheet>