<xsl:stylesheet
    xmlns:xs="http://www.w3.org/2001/XMLSchema"
    xmlns:xsl="http://www.w3.org/1999/XSL/Transform" version="2.0"
    exclude-result-prefixes="xs">

  <xsl:output method="xml" encoding="UTF-8" indent="yes" />

  <xsl:variable name="functionPrefix">
    <xsl:value-of select="'svF'" />
  </xsl:variable>

  <xsl:variable name="globalVariablePrefix">
    <xsl:value-of select="'svG'"/>
  </xsl:variable>

  <xsl:variable name="globalCodePrefix">
    <xsl:value-of select="'svGC'" />
  </xsl:variable>

  <xsl:variable name="Functions">
    <xsl:element name="Function">
      <xsl:attribute name="FunctionName" select="'isNumber'" />
      <xsl:attribute name="FunctionType" select="'global'" />
      <xsl:element name="Params">
        <xsl:element name="Param">n</xsl:element>
      </xsl:element>
      <xsl:element name="FunctionBody">
        <xsl:element name="Code">var exp = /^[1-9]?[0-9]*?(0?\.?[0-9]+)$/;</xsl:element>
        <xsl:element name="Code">return exp.test(n);</xsl:element>
      </xsl:element>
    </xsl:element>

    <xsl:element name="Function">
      <xsl:attribute name="FunctionName" select="'validateSurvey'" />
      <xsl:attribute name="FunctionType" select="'global'" />
      <xsl:element name="Params" />
      <xsl:element name="FunctionBody">
        <xsl:element name="Code">var nErrors = 0;</xsl:element>
        <xsl:element name="Code">var ctr = 0;</xsl:element>
        <xsl:element name="Code">while (ctr &lt; questionListNode.childNodes.length) {</xsl:element>
        <xsl:element name="Code">if (questionListNode.childNodes[ctr].className == "Error")</xsl:element>
        <xsl:element name="Code">questionListNode.removeChild(questionListNode.childNodes[ctr]);</xsl:element>
        <xsl:element name="Code">else</xsl:element>
        <xsl:element name="Code">ctr++;</xsl:element>
        <xsl:element name="Code">}</xsl:element>
        <xsl:element name="Code">nErrors = 0;</xsl:element>
        <xsl:element name="Code">
          <xsl:value-of select="concat('var uniqueNum = ', //Survey/@UniqueResponseItem, ';')"/>
        </xsl:element>
        <xsl:element name="Code">for (ctr = 0; ctr &lt; validateFunctions.length; ctr++) {</xsl:element>
        <xsl:element name="Code">var itemErrors = validateFunctions[ctr].call();</xsl:element>
        <xsl:element name="Code">if (itemErrors != 0)</xsl:element>
        <xsl:element name="Code">initFunctions[ctr].call();</xsl:element>
        <xsl:if test="//Survey/@UniqueResponseItem ne '-1'">
          <xsl:element name="Code">else if (uniqueNum == ctr + 1) {</xsl:element>
          <xsl:element name="Code">var errorMsgText, questionNode;</xsl:element>
          <xsl:element name="Code">if (UniqueResponseViolation == "consumed") {</xsl:element>
          <xsl:element name="Code">itemErrors = 1;</xsl:element>
          <xsl:element name="Code">errorMsgText = document.createTextNode("Invalid Response. The value you supplied has already been used.");</xsl:element>
          <xsl:element name="Code">uniqueRespErrorLI = document.createElement("li");</xsl:element>
          <xsl:element name="Code">uniqueRespErrorLI.className = "Error";</xsl:element>
          <xsl:element name="Code">
            <xsl:value-of select="concat('questionNode = document.getElementById(&quot;ItemLITag', //Survey/@UniqueResponseItem, '&quot;);')" />
          </xsl:element>
          <xsl:element name="Code">uniqueRespErrorLI.appendChild(errorMsgText);</xsl:element>
          <xsl:element name="Code">questionListNode.insertBefore(uniqueRespErrorLI, questionNode);</xsl:element>
          <xsl:element name="Code">} else if (UniqueResponseViolation == "invalid") {</xsl:element>
          <xsl:element name="Code">itemErrors = 1;</xsl:element>
          <xsl:element name="Code">errorMsgText = document.createTextNode("Invalid Response.");</xsl:element>
          <xsl:element name="Code">uniqueRespErrorLI = document.createElement("li");</xsl:element>
          <xsl:element name="Code">uniqueRespErrorLI.className = "Error";</xsl:element>
          <xsl:element name="Code">
            <xsl:value-of select="concat('questionNode = document.getElementById(&quot;ItemLITag', //Survey/@UniqueResponseItem, '&quot;);')" />
          </xsl:element>
          <xsl:element name="Code">uniqueRespErrorLI.appendChild(errorMsgText);</xsl:element>
          <xsl:element name="Code">questionListNode.insertBefore(uniqueRespErrorLI, questionNode);</xsl:element>
          <xsl:element name="Code">} else if (UniqueResponseViolation == "taken") {</xsl:element>
          <xsl:element name="Code">itemErrors = 1;</xsl:element>
          <xsl:element name="Code">errorMsgText = document.createTextNode("This respone has been supplied but the person who gave it has not completed the test. If it was you who recently supplied this answer and you abandoned the test, please wait fifteen minutes from the time you abandoned the test and try again.");</xsl:element>
          <xsl:element name="Code">uniqueRespErrorLI = document.createElement("li");</xsl:element>
          <xsl:element name="Code">uniqueRespErrorLI.className = "Error";</xsl:element>
          <xsl:element name="Code">
            <xsl:value-of select="concat('questionNode = document.getElementById(&quot;ItemLITag', //Survey/@UniqueResponseItem, '&quot;);')" />
          </xsl:element>
          <xsl:element name="Code">uniqueRespErrorLI.appendChild(errorMsgText);</xsl:element>
          <xsl:element name="Code">questionListNode.insertBefore(uniqueRespErrorLI, questionNode);</xsl:element>
          <xsl:element name="Code">}}</xsl:element>
        </xsl:if>
        <xsl:element name="Code">nErrors += itemErrors;</xsl:element>
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
        <xsl:element name="Code">if (nErrors &gt; 0) {</xsl:element>
        <xsl:element name="Code">var e = EventUtil.getEvent(event);</xsl:element>
        <xsl:element name="Code">if (ErrorsExistDiv == null) {</xsl:element>
        <xsl:element name="Code">ErrorsExistDiv = document.getElementById("ErrorsExistMsgDiv");</xsl:element>
        <xsl:element name="Code">var ErrorsExistMsg = document.createElement("h3");</xsl:element>
        <xsl:element name="Code">var ErrorsExistMsgText = document.createTextNode("Response errors detected. Please review the above survey for error messages and then resubmit.");</xsl:element>
        <xsl:element name="Code">ErrorsExistMsg.appendChild(ErrorsExistMsgText);</xsl:element>
        <xsl:element name="Code">ErrorsExistDiv.appendChild(ErrorsExistMsg);</xsl:element>
        <xsl:element name="Code">}</xsl:element>
        <xsl:element name="Code">EventUtil.preventDefault(e);</xsl:element>
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
    <xsl:apply-templates select="//SurveyItem" />
  </xsl:variable>

  <xsl:variable name="VariableDeclarations">
    <Declarations>
      <Declaration>var ForceSubmit = false;</Declaration>
      <Declaration>var questionListNode = document.getElementById("QuestionList");</Declaration>
      <Declaration>var initFunctions = new Array();</Declaration>
      <Declaration>var validateFunctions = new Array();</Declaration>
      <Declaration>var ErrorsExistDiv = null;</Declaration>
      <Declaration>var form = document.getElementById("SurveyForm");</Declaration>
      <Declaration>var uniqueRespErrorLI = null;</Declaration>
    </Declarations>
  </xsl:variable>

  <xsl:variable name="GlobalAbbreviations">
    <xsl:variable name="Globals" select="string-join(for $elem in $VariableDeclarations/Declarations/Declaration return replace($elem, '^var\s+(.+);$', '$1'), ', ')" />
    <xsl:analyze-string select="$Globals" regex="([A-Za-z_][A-Za-z0-9_]*)(\s*(=((\s+|[^;=/,&#34;\(]+?|&#34;[^&#34;\n\r]*?&#34;|\(([^;=,&#34;]*?,?(&#34;[^\n\r&#34;]*?&#34;)?)+\)|/[^/\n]+?/)*)+?)?)">
      <xsl:matching-substring>
        <xsl:element name="Entry">
          <xsl:attribute name="type" select="'global'" />
          <xsl:element name="OrigName">
            <xsl:value-of select="regex-group(1)" />
          </xsl:element>
          <xsl:element name="NewName">
            <xsl:value-of select="concat('_', $globalVariablePrefix, position())" />
          </xsl:element>
          <xsl:element name="Assign">
            <xsl:value-of select="normalize-space(regex-group(4))" />
          </xsl:element>
        </xsl:element>
      </xsl:matching-substring>
    </xsl:analyze-string>
  </xsl:variable>

  <xsl:variable name="GlobalCode">
    <xsl:element name="Code">EventUtil.addHandler(form, "submit", OnSubmit);</xsl:element>
    <xsl:if test="//Survey/@TimeoutMillis ne '0'">
      <xsl:element name="Code">
        <xsl:value-of select="concat('setTimeout(DoForceSubmit, ', //Survey/@TimeoutMillis, ');')"/>
      </xsl:element>
    </xsl:if>
    <xsl:for-each select="for $i in 1 to count(//SurveyItem/Response[@Type ne 'Instruction']) return $i">
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
        <xsl:for-each select="$Functions/Function">
          <xsl:variable name="nodeName" select="name()" />
          <xsl:element name="{$nodeName}">
            <xsl:for-each select="attribute::*">
              <xsl:copy-of select="." />
            </xsl:for-each>
            <xsl:attribute name="FunctionPrefix" select="$functionPrefix" />
            <xsl:copy-of select="child::*" />
          </xsl:element>
        </xsl:for-each>
      </xsl:element>
      <xsl:element name="GlobalCode">
        <xsl:attribute name="CodePrefix" select="$globalCodePrefix" />
        <xsl:copy-of select="$GlobalCode" />
      </xsl:element>
    </xsl:element>
  </xsl:template>

  <xsl:template match="SurveyItem">
    <xsl:variable name="precedingNodes" select="preceding-sibling::node()" />
    <xsl:variable name="precedingSurveyItems" select="$precedingNodes[compare(name(), 'SurveyItem') eq 0]" />
    <xsl:variable name="itemNum" select="count($precedingSurveyItems/Response[compare(@Type, 'Instruction') ne 0]) + 1" />
    <xsl:if test="Response/@Type ne 'Instruction'" >
      <xsl:apply-templates select="Response">
        <xsl:with-param name="itemNum" as="xs:integer" select="$itemNum" />
        <xsl:with-param name="liNum" as="xs:integer" select="count($precedingSurveyItems)" />
      </xsl:apply-templates>
    </xsl:if>
  </xsl:template>

  <xsl:template match="Response[@Type='Likert']">
    <xsl:param name="itemNum" as="xs:integer" />
    <xsl:param name="liNum" as="xs:integer" />

    <xsl:element name="Function">
      <xsl:attribute name="FunctionName" select="concat('InitializeItem', $itemNum)" />
      <xsl:attribute name="FunctionType" select="'initialization'" />
      <xsl:element name="Params" />
      <xsl:element name="FunctionBody">
        <xsl:element name="Code">
          <xsl:value-of select="concat('var answerInputs = document.getElementsByName(&quot;Item', $itemNum, '&quot;);')" />
        </xsl:element>
        <xsl:element name="Code">for (var ctr = 0; ctr &lt; answerInputs.length; ctr++)</xsl:element>
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
        <xsl:element name="Code">var ctr;</xsl:element>
        <xsl:element name="Code">var selectionMade = false;</xsl:element>
        <xsl:element name="Code">for (ctr = 0; ctr &lt; answerInputs.length; ctr++)</xsl:element>
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

  <xsl:template match="Response[@Type='Boolean']" >
    <xsl:param name="itemNum" as="xs:integer" />
    <xsl:param name="liNum" as="xs:integer" />

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
        <xsl:element name="Code">for (var ctr = 0; ctr &lt; answerInputs.length; ctr++)</xsl:element>
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
        <xsl:element name="Code">for (ctr = 0; ctr &lt; answerInputs.length; ctr++)</xsl:element>
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

  <xsl:template match="Response[@Type='Multiple Choice']" >
    <xsl:param name="itemNum" as="xs:integer" />
    <xsl:param name="liNum" as="xs:integer" />

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
        <xsl:element name="Code">for (var ctr = 0; ctr &lt; answerInputs.length; ctr++)</xsl:element>
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
        <xsl:element name="Code">for (ctr = 0; ctr &lt; answerInputs.length; ctr++)</xsl:element>
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


  <xsl:template match="Response[@Type='Weighted Multiple Choice']" >
    <xsl:param name="itemNum" as="xs:integer" />
    <xsl:param name="liNum" as="xs:integer" />

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
        <xsl:element name="Code">for (var ctr = 0; ctr &lt; answerInputs.length; ctr++)</xsl:element>
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
        <xsl:element name="Code">for (ctr = 0; ctr &lt; answerInputs.length; ctr++)</xsl:element>
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

  <xsl:template match="Response[@Type='Multiple Selection']" >
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
          <xsl:value-of select="concat('for (ctr = 0; ctr &lt; ', count(./Labels/Label), '; ctr++) {')" />
        </xsl:element>
        <xsl:element name="Code">
          <xsl:value-of select="concat('var selectionInput = document.getElementById(&quot;Item', $itemNum, '_&quot;.concat((ctr + 1).toString()));')" />
        </xsl:element>
        <xsl:element name="Code">if (selectionInput.checked)</xsl:element>
        <xsl:element name="Code">nChecked++;</xsl:element>
        <xsl:element name="Code">}</xsl:element>
        <xsl:element name="Code">var errorMsgLI, errorMsg;</xsl:element>
        <xsl:element name="Code">
          <xsl:value-of select="concat('if (nChecked &lt; ', ./MinSelections, ') {')"/>
        </xsl:element>
        <xsl:element name="Code">if (!ForceSubmit) {</xsl:element>
        <xsl:element name="Code">errorMsgLI = document.createElement("li");</xsl:element>
        <xsl:element name="Code">errorMsgLI.className = "Error";</xsl:element>
        <xsl:if test="./MinSelections eq '1'" >
          <xsl:element name="Code">
            <xsl:value-of select="concat('errorMsg = document.createTextNode(&quot;Please select at least ', ./MinSelections, ' response to the question below.&quot;);')" />
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
          <xsl:value-of select="concat('} else if (nChecked &gt; ', ./MaxSelections, ') {')" />
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
          <xsl:value-of select="concat('for (ctr = 0; ctr &lt; ', count(./Labels/Label), '; ctr++) {')" />
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

  <xsl:template match="Response[@Type='Date']" >
    <xsl:param name="itemNum" as="xs:integer" />
    <xsl:param name="liNum" as="xs:integer" />

    <xsl:element name="Function">
      <xsl:attribute name="FunctionName">
        <xsl:value-of select="concat('InitializeItem', $itemNum)" />
      </xsl:attribute>
      <xsl:attribute name="FunctionType" select="'initialization'" />
      <xsl:element name="Params" />
      <xsl:element name="FunctionBody">
        <xsl:element name="Code">
          <xsl:value-of select="concat('var answerInput = document.getElementById(&quot;Item', $itemNum, '&quot;);')" />
        </xsl:element>
        <xsl:element name="Code">answerInput.value = "";</xsl:element>
      </xsl:element>
    </xsl:element>

    <xsl:element name="Function">
      <xsl:attribute name="FunctionName">
        <xsl:value-of select="concat('ValidateItem', $itemNum)"/>
      </xsl:attribute>
      <xsl:attribute name="FunctionType" select="'validation'" />
      <xsl:element name="Params" />
      <xsl:element name="FunctionBody">
        <xsl:element name="Code">var daysInMonths = new Array();</xsl:element>
        <xsl:element name="Code">daysInMonths.splice(0, 0, 31, 29, 31, 30, 31, 30, 31, 31, 30, 31, 30, 31);</xsl:element>
        <xsl:element name="Code">
          <xsl:value-of select="concat('var questionLI = document.getElementById(&quot;ItemLITag', $liNum, '&quot;);')" />
        </xsl:element>
        <xsl:element name="Code">
          <xsl:value-of select="concat('var answerInput = document.getElementById(&quot;Item', $itemNum, '&quot;);')" />
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
        <xsl:element name="Code">else if ((month &lt; 1) || (month &gt; 12))</xsl:element>
        <xsl:element name="Code">error = 2;</xsl:element>
        <xsl:element name="Code">else if ((day &lt; 1) || (day &gt; daysInMonths[month - 1]))</xsl:element>
        <xsl:element name="Code">error = 3;</xsl:element>
        <xsl:element name="Code">else if ((day == 29) &amp;&amp; (month == 2) &amp;&amp; (year % 4))</xsl:element>
        <xsl:element name="Code">error = 3;</xsl:element>
        <xsl:element name="Code">else if (year &lt; 0)</xsl:element>
        <xsl:element name="Code">error = 4;</xsl:element>
        <xsl:if test="./StartDate[@HasValue eq 'True']" >
          <xsl:element name="Code">
            <xsl:value-of select="concat('if (year &lt; ', ./StartDate/Year, ')')" />
          </xsl:element>
          <xsl:element name="Code">error = 5;</xsl:element>
          <xsl:element name="Code">
            <xsl:value-of select="concat('else if (year == ', StartDate/Year, ') {')" />
          </xsl:element>
          <xsl:element name="Code">
            <xsl:value-of select="concat('if (month &lt; ', StartDate/Month, ')')"/>
          </xsl:element>
          <xsl:element name="Code">error = 5;</xsl:element>
          <xsl:element name="Code">
            <xsl:value-of select="concat('else if (month == ', StartDate/Month, ')')"/>
          </xsl:element>
          <xsl:element name="Code">
            <xsl:value-of select="concat('if (day &lt; ', StartDate/Day, ')')"/>
          </xsl:element>
          <xsl:element name="Code">error = 5;</xsl:element>
          <xsl:element name="Code">}</xsl:element>
        </xsl:if>
        <xsl:if test="./EndDate[@HasValue eq 'True']" >
          <xsl:element name="Code">
            <xsl:value-of select="concat('if (year &gt; ', EndDate/Year, ')')"/>
          </xsl:element>
          <xsl:element name="Code">error = 6;</xsl:element>
          <xsl:element name="Code">
            <xsl:value-of select="concat('else if (year == ', EndDate/Year, ') {')" />
          </xsl:element>
          <xsl:element name="Code">
            <xsl:value-of select="concat('if (month &gt; ', EndDate/Month, ')')" />
          </xsl:element>
          <xsl:element name="Code">error = 6;</xsl:element>
          <xsl:element name="Code">
            <xsl:value-of select="concat('else if (month == ', EndDate/Month, ')')" />
          </xsl:element>
          <xsl:element name="Code">
            <xsl:value-of select="concat('if (day &gt; ', EndDate/Day, ')')"/>
          </xsl:element>
          <xsl:element name="Code">error = 6;</xsl:element>
          <xsl:element name="Code">}</xsl:element>
        </xsl:if>
        <xsl:element name="Code">}</xsl:element>
        <xsl:element name="Code">var monthNames = new Array();</xsl:element>
        <xsl:element name="Code">monthNames.push("January");</xsl:element>
        <xsl:element name="Code">monthNames.push("February");</xsl:element>
        <xsl:element name="Code">monthNames.push("March");</xsl:element>
        <xsl:element name="Code">monthNames.push("April");</xsl:element>
        <xsl:element name="Code">monthNames.push("May");</xsl:element>
        <xsl:element name="Code">monthNames.push("April");</xsl:element>
        <xsl:element name="Code">monthNames.push("May");</xsl:element>
        <xsl:element name="Code">monthNames.push("June");</xsl:element>
        <xsl:element name="Code">monthNames.push("August");</xsl:element>
        <xsl:element name="Code">monthNames.push("September");</xsl:element>
        <xsl:element name="Code">monthNames.push("October");</xsl:element>
        <xsl:element name="Code">monthNames.push("November");</xsl:element>
        <xsl:element name="Code">monthNames.push("December");</xsl:element>
        <xsl:element name="Code">if (error != 0) {</xsl:element>
        <xsl:element name="Code">if (!ForceSubmit) {</xsl:element>
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
        <xsl:element name="Code">}} else if (error != 0) {</xsl:element>
        <xsl:element name="Code">answerInput.value = "NULL";</xsl:element>
        <xsl:element name="Code">return 1;</xsl:element>
        <xsl:element name="Code">}</xsl:element>
        <xsl:element name="Code">return 0;</xsl:element>
      </xsl:element>
    </xsl:element>
  </xsl:template>

  <xsl:template match="Response[@Type='Bounded Length']" >
    <xsl:param name="itemNum" as="xs:integer" />
    <xsl:param name="liNum" as="xs:integer" />

    <xsl:element name="Function">
      <xsl:attribute name="FunctionName">
        <xsl:value-of select="concat('InitializeItem', $itemNum)"/>
      </xsl:attribute>
      <xsl:attribute name="FunctionType" select="'initialization'" />
      <xsl:element name="Params" />
      <xsl:element name="FunctionBody">
        <xsl:element name="Code">
          <xsl:value-of select="concat('var answerInput = document.getElementById(&quot;Item', $itemNum, '&quot;);')" />
        </xsl:element>
        <xsl:element name="Code">answerInput.value = "";</xsl:element>
      </xsl:element>
    </xsl:element>

    <xsl:element name="Function">
      <xsl:attribute name="FunctionName">
        <xsl:value-of select="concat('ValidateItem', $itemNum)"/>
      </xsl:attribute>
      <xsl:attribute name="FunctionType" select="'validation'" />
      <xsl:element name="Params" />
      <xsl:element name="FunctionBody">
        <xsl:element name="Code">
          <xsl:value-of select="concat('var answerInput = document.getElementById(&quot;Item', $itemNum, '&quot;);')" />
        </xsl:element>
        <xsl:element name="Code">var answer = answerInput.value;</xsl:element>
        <xsl:element name="Code">
          <xsl:value-of select="concat('if ((answer.length &lt; ', MinLength, ') || (answer.length &gt; ', MaxLength, ')) {')" />
        </xsl:element>
        <xsl:element name="Code">if (!ForceSubmit) {</xsl:element>
        <xsl:element name="Code">
          <xsl:value-of select="concat('var questionLI = document.getElementById(&quot;ItemLITag', $liNum, '&quot;);')" />
        </xsl:element>
        <xsl:element name="Code">var errorMsgLI = document.createElement("li");</xsl:element>
        <xsl:element name="Code">errorMsgLI.className = "Error";</xsl:element>
        <xsl:element name="Code">
          <xsl:value-of select="concat('var errorMsg = document.createTextNode(&quot;Please provide a response between ', MinLength, ' and ', MaxLength, ' characters in length for the question below.&quot;);')" />
        </xsl:element>
        <xsl:element name="Code">errorMsgLI.appendChild(errorMsg);</xsl:element>
        <xsl:element name="Code">questionListNode.insertBefore(errorMsgLI, questionLI);</xsl:element>
        <xsl:element name="Code">return 1;</xsl:element>
        <xsl:element name="Code">} else {</xsl:element>
        <xsl:element name="Code">answerInput.value = "NULL";</xsl:element>
        <xsl:element name="Code">return 1;</xsl:element>
        <xsl:element name="Code">}}</xsl:element>
        <xsl:element name="Code">return 0;</xsl:element>
        <xsl:element name="Code"></xsl:element>
        <xsl:element name="Code"></xsl:element>
      </xsl:element>
    </xsl:element>
  </xsl:template>

  <xsl:template match="Response[@Type='Bounded Number']" >
    <xsl:param name="itemNum" as="xs:integer" />
    <xsl:param name="liNum" as="xs:integer" />

    <xsl:element name="Function">
      <xsl:attribute name="FunctionName">
        <xsl:value-of select="concat('InitializeItem', $itemNum)"/>
      </xsl:attribute>
      <xsl:attribute name="FunctionType" select="'initialization'" />
      <xsl:element name="Params" />
      <xsl:element name="FunctionBody">
        <xsl:element name="Code">
          <xsl:value-of select="concat('var answerInput = document.getElementById(&quot;Item', $itemNum, '&quot;);')" />
        </xsl:element>
        <xsl:element name="Code">answerInput.value = "";</xsl:element>
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
          <xsl:value-of select="concat('var answerInput = document.getElementById(&quot;Item', $itemNum, '&quot;);')"/>
        </xsl:element>
        <xsl:element name="Code">var answer = answerInput.value;</xsl:element>
        <xsl:element name="Code">if (!isNumber(answer)) {</xsl:element>
        <xsl:element name="Code">if (!ForceSubmit) {</xsl:element>
        <xsl:element name="Code">var errorMsgLI = document.createElement("li");</xsl:element>
        <xsl:element name="Code">
          <xsl:value-of select="concat('var questionLI = document.getElementById(&quot;ItemLITag', $liNum, '&quot;);')" />
        </xsl:element>
        <xsl:element name="Code">errorMsgLI.className = "Error";</xsl:element>
        <xsl:element name="Code">errorMsgLI.appendChild(document.createTextNode("Please enter a numerical response for the question below."));</xsl:element>
        <xsl:element name="Code">questionListNode.insertBefore(errorMsgLI, questionLI);</xsl:element>
        <xsl:element name="Code">return 1;</xsl:element>
        <xsl:element name="Code">} else {</xsl:element>
        <xsl:element name="Code">answerInput.value = "NULL";</xsl:element>
        <xsl:element name="Code">return 1;</xsl:element>
        <xsl:element name="Code">}}</xsl:element>
        <xsl:element name="Code">var num;</xsl:element>
        <xsl:element name="Code">if (!answer)</xsl:element>
        <xsl:element name="Code">num = Number.NaN;</xsl:element>
        <xsl:element name="Code">else</xsl:element>
        <xsl:element name="Code">num = parseFloat(answer);</xsl:element>
        <xsl:element name="Code">
          <xsl:value-of select="concat('var questionLI = document.getElementById(&quot;ItemLITag', $liNum, '&quot;);')"/>
        </xsl:element>
        <xsl:element name="Code">var errorMsgLI, errorMsg;</xsl:element>
        <xsl:element name="Code">if (isNaN(num)) {</xsl:element>
        <xsl:element name="Code">if (!ForceSubmit) {</xsl:element>
        <xsl:element name="Code">errorMsgLI = document.createElement("li");</xsl:element>
        <xsl:element name="Code">errorMsgLI.className = "Error";</xsl:element>
        <xsl:element name="Code">errorMsg = document.createTextNode("Please enter a numerical response for the question below.");</xsl:element>
        <xsl:element name="Code">errorMsgLI.appendChild(errorMsg);</xsl:element>
        <xsl:element name="Code">questionListNode.insertBefore(errorMsgLI, questionLI);</xsl:element>
        <xsl:element name="Code">return 1;</xsl:element>
        <xsl:element name="Code">} else {</xsl:element>
        <xsl:element name="Code">answerInput.value = "NULL";</xsl:element>
        <xsl:element name="Code">return 1;</xsl:element>
        <xsl:element name="Code">}}</xsl:element>
        <xsl:element name="Code">
          <xsl:value-of select="concat('else if ((num &lt; ', MinValue, ') || (num &gt; ', MaxValue, ')) {')" />
        </xsl:element>
        <xsl:element name="Code">if (!ForceSubmit) {</xsl:element>
        <xsl:element name="Code">errorMsgLI = document.createElement("li");</xsl:element>
        <xsl:element name="Code">errorMsgLI.className = "Error";</xsl:element>
        <xsl:element name="Code">
          <xsl:value-of select="concat('errorMsg = document.createTextNode(&quot;Please enter a numerical response between ', MinValue, ' and ', MaxValue, ' for the question below.&quot;);')" />
        </xsl:element>
        <xsl:element name="Code">errorMsgLI.appendChild(errorMsg);</xsl:element>
        <xsl:element name="Code">questionListNode.insertBefore(errorMsgLI, questionLI);</xsl:element>
        <xsl:element name="Code">return 1;</xsl:element>
        <xsl:element name="Code">} else {</xsl:element>
        <xsl:element name="Code">answerInput.value = "NULL";</xsl:element>
        <xsl:element name="Code">return 1;</xsl:element>
        <xsl:element name="Code">}}</xsl:element>
        <xsl:element name="Code">return 0;</xsl:element>
      </xsl:element>
    </xsl:element>
  </xsl:template>

  <xsl:template match="Response[@Type='Fixed Digit']" >
    <xsl:param name="itemNum" as="xs:integer" />
    <xsl:param name="liNum" as="xs:integer" />

    <xsl:element name="Function">
      <xsl:attribute name="FunctionName">
        <xsl:value-of select="concat('InitializeItem', $itemNum)" />
      </xsl:attribute>
      <xsl:attribute name="FunctionType" select="'initialization'" />
      <xsl:element name="Params" />
      <xsl:element name="FunctionName">
        <xsl:element name="Code">
          <xsl:value-of select="concat('var answerInput = document.getElementById(&quot;Item', $itemNum, ');')" />
        </xsl:element>
        <xsl:element name="Code">answerInput.value = "";</xsl:element>
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
          <xsl:value-of select="concat('var answerInput = document.getElementById(&quot;Item', $itemNum, '&quot;);')" />
        </xsl:element>
        <xsl:element name="Code">var answer = answerInput.value;</xsl:element>
        <xsl:element name="Code">var num = parseInt(answer, 10);</xsl:element>
        <xsl:element name="Code">var errorMsgLI, errorMsg;</xsl:element>
        <xsl:element name="Code">var bErrorInResponse = false;</xsl:element>
        <xsl:element name="Code">
          <xsl:value-of select="concat('var intExp = /[0-9]{', NumDigs, '}/;')" />
        </xsl:element>
        <xsl:element name="Code">if (!intExp.test(num))</xsl:element>
        <xsl:element name="Code">bErrorInResponse = true;</xsl:element>
        <xsl:element name="Code">if (bErrorInResponse) {</xsl:element>
        <xsl:element name="Code">if (!ForceSubmit) {</xsl:element>
        <xsl:element name="Code">errorMsgLI = document.createElement("li");</xsl:element>
        <xsl:element name="Code">errorMsgLI.className = "Error";</xsl:element>
        <xsl:element name="Code">
          <xsl:value-of select="concat('errorMsg = document.createTextNode(&quot;Please enter a numerical response of ', NumDigs, ' digits for the question below.&quot;);')" />
        </xsl:element>
        <xsl:element name="Code">errorMsgLI.appendChild(errorMsg);</xsl:element>
        <xsl:element name="Code">questionListNode.insertBefore(errorMsgLI, questionLI);</xsl:element>
        <xsl:element name="Code">return 1;</xsl:element>
        <xsl:element name="Code">} else {</xsl:element>
        <xsl:element name="Code">answerInput.value = "NULL";</xsl:element>
        <xsl:element name="Code">return 1;</xsl:element>
        <xsl:element name="Code">}}</xsl:element>
        <xsl:element name="Code">return 0;</xsl:element>
      </xsl:element>
    </xsl:element>
  </xsl:template>

  <xsl:template match="Response[@Type='Regular Expression']" >
    <xsl:param name="itemNum" as="xs:integer" />
    <xsl:param name="liNum" as="xs:integer" />

    <xsl:element name="Function">
      <xsl:attribute name="FunctionName">
        <xsl:value-of select="concat('InitializeItem', $itemNum)"/>
      </xsl:attribute>
      <xsl:attribute name="FunctionType" select="'initialization'" />
      <xsl:element name="Params" />
      <xsl:element name="FunctionBody">
        <xsl:element name="Code">
          <xsl:value-of select="concat('var answerInput = document.getElementById(&quot;Item', $itemNum, '&quot;);')"/>
        </xsl:element>
        <xsl:element name="Code">answerInput.value = "";</xsl:element>
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
          <xsl:value-of select="concat('var answerInput = document.getElementById(&quot;Item', $itemNum, '&quot;);')" />
        </xsl:element>
        <xsl:element name="Code">var answer = answerInput.value;</xsl:element>
        <xsl:element name="Code">
          <xsl:value-of select="concat('var regEx = /', Expression, '/;')" />
        </xsl:element>
        <xsl:element name="Code">if (answer.search(regEx) != 0) {</xsl:element>
        <xsl:element name="Code">if (!ForceSubmit) {</xsl:element>
        <xsl:element name="Code">
          <xsl:value-of select="concat('var questionLI = document.getElementById(&quot;ItemLITag', $liNum, '&quot;);')" />
        </xsl:element>
        <xsl:element name="Code">var errorMsgLI = document.createElement("li");</xsl:element>
        <xsl:element name="Code">errorMsgLI.className = "Error";</xsl:element>
        <xsl:element name="Code">var errorMsg = document.createTextNode("Invalid input supplied for the question below.");</xsl:element>
        <xsl:element name="Code">errorMsgLI.appendChild(errorMsg);</xsl:element>
        <xsl:element name="Code">questionListNode.insertBefore(errorMsgLI, questionLI);</xsl:element>
        <xsl:element name="Code">return 1;</xsl:element>
        <xsl:element name="Code">} else {</xsl:element>
        <xsl:element name="Code">answerInput.value = "NULL";</xsl:element>
        <xsl:element name="Code">return 1;</xsl:element>
        <xsl:element name="Code">}}</xsl:element>
        <xsl:element name="Code">return 0;</xsl:element>
      </xsl:element>
    </xsl:element>
  </xsl:template>
</xsl:stylesheet>