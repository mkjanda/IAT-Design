<xsl:stylesheet
    xmlns:xs="http://www.w3.org/2001/XMLSchema"
    xmlns:xsl="http://www.w3.org/1999/XSL/Transform" version="2.0"
    exclude-result-prefixes="xs">

  <xsl:template match="Survey">
    <html>
      <head>
        <title>
          <xsl:variable name="caption" select="./Caption" />
          <xsl:if test="count($caption) eq 1" >
            <xsl:value-of select="./Caption/Text" />
          </xsl:if>
          <xsl:if test="count($caption) eq 0" >
            <xsl:value-of select="@IAT" />
          </xsl:if>
        </title>
        <style type="text/css">
          body {
          font: 100% Verdana, Arial, Helvetica, sans-serif;
          background: #FFFFFF;
          margin: 0;
          padding: 0;
          text-align: center;
          color: #000000;
          }

          .oneColFixCtrHdr #container {
          width: 780px;  /* using 20px less than a full 800px width allows for browser chrome and avoids a horizontal scroll bar */
          background: #FFFFFF;
          margin: 0 auto; /* the auto margins (in conjunction with a width) center the page */
          text-align: left; /* this overrides the text-align: center on the body element. */
          }

          <xsl:if test="count(./Caption) eq 1" >
            .oneColFixCtrHdr #header {
            background: #<xsl:value-of select="concat(./Caption/BackColorR, ./Caption/BackColorG, ./Caption/BackColorB)" />;
            padding: 0 10px 0 20px;
            margin-bottom: 30px;
            border-bottom: <xsl:value-of select="./Caption/BorderWidth" />px solid #<xsl:value-of select="concat(./Caption/BorderColorR, ./Caption/BorderColorG, ./Caption/BorderColorB)" />;
            }


            .oneColFixCtrHdr #header h1 {
            margin: 0;
            padding: 10px 0; /* using padding instead of margin will allow you to keep the element away from the edges of the div */
            text-align: center;
            font-family: "Times New Roman", Times, serif;
            font-size: <xsl:value-of select="./Caption/FontSize" />px;
            font-weight: bold;
            letter-spacing: 1px;
            color: #<xsl:value-of select="concat(./Caption/FontColorR, ./Caption/FontColorG, ./Caption/FontColorB)" />;
            }
          </xsl:if>


          .oneColFixCtrHdr #mainContent {
          padding: 0 20px; /* remember that padding is the space inside the div box and margin is the space outside the div box */
          background: #FFFFFF;
          }

          .oneColFixCtrHdr #mainContent ul {
          list-style: none;
          }

          .oneColFixCtrHdr #mainContent ul li {
          margin: 5px 0px 10px 0px;
          }

          .RadioInputCell {
          vertical-align: top;
          }

          .InstructionsDiv h3 {
          font-family: Arial, Helvetica, sans-serif;
          font-size: 16px;
          color: #000099;
          font-weight: normal;
          }

          .SurveyItemDiv h3 {
          font-family: Arial, Helvetica, sans-serif;
          text-indent: -15px;
          padding-left: 15px;
          font-size: 16px;
          color: #000000;
          font-weight: normal;
          margin: 2px 5px 3px 20px;
          }

          .SurveyItemDiv .RadioButtonTable {
          font-family: Arial, Helvetica, sans-serif;
          font-size: 14px;
          color: #000000;
          font-weight: normal;
          margin: 2px 5px 3px 40px;
          padding: 0px;
          }

          .SurveyItemDiv .CheckBoxTable {
          font-family: Arial, Helvetica, sans-serif;
          font-size: 14px;
          color: #000000;
          font-weight: normal;
          margin-left: 20px;
          }

          .RadioLabelParagraph {
          width: 500px;
          }

          .BoundedLengthTextArea {
          font-family: Arial, Helvetica, sans-serif;
          font-size: 14px;
          color: #666666;
          margin: 5px 5px 5px 35p;
          }

          #SubmitButton {
          width: 100px;
          margin-top: 20px;
          margin-left: 320px;
          margin-right: 300px;
          margin-bottom: 20px;
          }

          <xsl:for-each select="SurveyItem/Response[@Type = 'Bounded Length']">
            <xsl:variable name="precedingNodes" select="../preceding-sibling::node()" />
            <xsl:variable name="precedingSurveyItems" select="$precedingNodes[compare(name(), 'SurveyItem') eq 0]" />
            <xsl:variable name="itemNum" select="count($precedingSurveyItems/Response[compare(@Type, 'Instruction') ne 0])" />
            <xsl:variable name="responseID" select="concat('#Item', $itemNum)" />
            <xsl:variable name="maxLength" as="xs:integer" select="MaxLength" />
            .SurveyItemDiv <xsl:value-of select="$responseID" /> {
            <xsl:if test="$maxLength le 48" >
              width: <xsl:value-of select="ceiling($maxLength * 12.5)" />px;
            </xsl:if>
            <xsl:if test="$maxLength gt 48" >
              width: 600px;
            </xsl:if>
            }
          </xsl:for-each>

        </style>
      </head>
      <body class="oneColFixCtrHdr">
        <xsl:if test="count(./Caption) eq 1">
          <div id="header">
            <h1>
              <xsl:value-of select="./Caption/Text" />
            </h1>
          </div>
        </xsl:if>
        <div id="container">
          <div id="mainContent">
            <form method="post">
              <ul>
                <xsl:apply-templates select="SurveyItem" />
              </ul>
              <xsl:element name="input">
                <xsl:attribute name="id" select="'SubmitButton'" />
                <xsl:attribute name="type" select="'submit'" />
                <xsl:attribute name="value" select="'Submit'" />
              </xsl:element>
            </form>
          </div>
        </div>
      </body>
    </html>

  </xsl:template>

  <xsl:template match="SurveyItem">
    <xsl:variable name="precedingNodes" select="preceding-sibling::node()" />
    <xsl:variable name="precedingSurveyItems" select="$precedingNodes[compare(name(), 'SurveyItem') eq 0]" />
    <xsl:variable name="itemNum" select="count($precedingSurveyItems/Response[compare(@Type, 'Instruction') ne 0])" />
    <li>
      <xsl:element name="div">
        <xsl:if test="Response/@Type eq 'Instruction'" >
          <xsl:attribute name="class" select="'InstructionsDiv'" />
        </xsl:if>
        <xsl:if test="Response/@Type ne 'Instruction'" >
          <xsl:attribute name="class" select="'SurveyItemDiv'" />
        </xsl:if>
        <h3>
          <xsl:value-of select="Text" />
        </h3>
        <xsl:apply-templates select="Response">
          <xsl:with-param name="itemNum" as="xs:integer" select="$itemNum" />
        </xsl:apply-templates>
      </xsl:element>
    </li>
  </xsl:template>

  <xsl:template match="Response[@Type='Likert']">
    <xsl:param name="itemNum" as="xs:integer" />
    <xsl:variable name="reverseScored" select='./IsReverseScored' />
    <xsl:variable name="numChoices" select='xs:integer(./NumChoices)' />
    <xsl:element name="table" >
      <xsl:attribute name="width" select="'90%'" />
      <xsl:attribute name="class" select="'RadioButtonTable'" />
      <xsl:for-each select="./ChoiceDescriptions/Choice" >
        <xsl:call-template name="writeRadioButton">
          <xsl:with-param name="radioGroup" select="concat('Item', $itemNum)" />
          <xsl:with-param name="radioValue">
            <xs:if test='$reverseScored = "True"'>
              <xsl:value-of select='$numChoices + 1 - position()' />
            </xs:if>
            <xs:if test='$reverseScored = "False"'>
              <xsl:value-of select='position()' />
            </xs:if>
          </xsl:with-param>
          <xsl:with-param name="radioLabel" select="." />
        </xsl:call-template>
      </xsl:for-each>
    </xsl:element>
  </xsl:template>

  <xsl:template match="Response[@Type='Boolean']" >
    <xsl:param name="itemNum" as="xs:integer" />
    <xsl:element name="table" >
      <xsl:attribute name="width" select="'90%'" />
      <xsl:attribute name="class" select="'RadioButtonTable'" />
      <xsl:call-template name="writeRadioButton">
        <xsl:with-param name="radioGroup" select="concat('Item', $itemNum)" />
        <xsl:with-param name="radioValue" select="'1'" />
        <xsl:with-param name="radioLabel" select="./TrueStatement" />
      </xsl:call-template>
      <xsl:call-template name="writeRadioButton">
        <xsl:with-param name="radioGroup" select="concat('Item', $itemNum)" />
        <xsl:with-param name="radioValue" select="'0'" />
        <xsl:with-param name="radioLabel" select="./FalseStatement" />
      </xsl:call-template>
    </xsl:element>
  </xsl:template>

  <xsl:template match="Response[@Type='Multiple Choice']" >
    <xsl:param name="itemNum" as="xs:integer" />
    <xsl:element name="table" >
      <xsl:attribute name="width" select="'90%'" />
      <xsl:attribute name="class" select="'RadioButtonTable'" />
      <xsl:for-each select="./Choices/Choice" >
        <xsl:call-template name="writeRadioButton">
          <xsl:with-param name="radioGroup" select="concat('Item', $itemNum)" />
          <xsl:with-param name="radioValue" select="position()" />
          <xsl:with-param name="radioLabel" select="." />
        </xsl:call-template>
      </xsl:for-each>
    </xsl:element>
  </xsl:template>

  <xsl:template match="Response[@Type='Weighted Multiple Choice']" >
    <xsl:param name="itemNum" as="xs:integer" />
    <xsl:element name="table" >
      <xsl:attribute name="width" select="'90%'" />
      <xsl:attribute name="class" select="'RadioButtonTable'" />
      <xsl:for-each select="./WeightedChoices/WeightedChoice">
        <xsl:call-template name="writeRadioButton">
          <xsl:with-param name="radioGroup" select="concat('Item', $itemNum)" />
          <xsl:with-param name="radioValue" select="./Weight" />
          <xsl:with-param name="radioLabel" select="./Choice" />
        </xsl:call-template>
      </xsl:for-each>
    </xsl:element>
  </xsl:template>

  <xsl:template match="Response[@Type='Multiple Selection']" >
    <xsl:param name="itemNum" as="xs:integer" />
    <xsl:variable name="numTableRows" select="xs:integer(ceiling((count(./Labels/Label)) div 2))" />
    <xsl:variable name="labels" select="./Labels" />
    <xsl:element name="table">
      <xsl:attribute name="width" select="'90%'" />
      <xsl:attribute name="class" select="'CheckBoxTable'" />
      <xsl:for-each select="1 to $numTableRows" >
        <xsl:variable name="col1Index" select="position()" />
        <xsl:variable name="col2Index" select="position() + $numTableRows" />
        <tr>
          <td>
            <xsl:element name="input">
              <xsl:attribute name="type" select="'checkbox'" />
              <xsl:attribute name="name" select="concat('Item', $itemNum, '_', $col1Index)" />
              <xsl:attribute name="ID" select="concat('Item', $itemNum, '_', $col1Index)" />
            </xsl:element>
            <xsl:element name="label">
              <xsl:attribute name="for" select="concat('Item', $itemNum, '_', $col1Index)" />
              <xsl:value-of select="$labels/Label[position() = $col1Index]" />
            </xsl:element>
          </td>
          <xsl:if test="position() + $numTableRows le count($labels/Label)">
            <td>
              <xsl:element name="input">
                <xsl:attribute name="type" select="'checkbox'" />
                <xsl:attribute name="name" select="concat('Item', $itemNum, '_', $col2Index)" />
                <xsl:attribute name="ID" select="concat('Item', $itemNum, '_', $col2Index)" />
              </xsl:element>
              <xsl:element name="label">
                <xsl:attribute name="for" select="concat('Item', $itemNum, '_', $col1Index)" />
                <xsl:value-of select="$labels/Label[position() = $col2Index]" />
              </xsl:element>
            </td>

          </xsl:if>
        </tr>
      </xsl:for-each>
    </xsl:element>
  </xsl:template>

  <xsl:template match="Response[@Type='Date']" >
    <xsl:param name="itemNum" as="xs:integer" />
    <xsl:element name="input">
      <xsl:attribute name="type" select="'text'" />
      <xsl:attribute name="name" select="concat('Item', $itemNum)" />
      <xsl:attribute name="ID" select="concat('Item', $itemNum)" />
      <xsl:attribute name="class" select="'DateInput'" />
    </xsl:element>
    <xsl:element name="label">
      <xsl:attribute name="for" select="concat('Item', $itemNum)" />
      <xsl:attribute name="class" select="'DateLabel'" />
      Please enter a date in MM/DD/YYYY format.
    </xsl:element>
  </xsl:template>

  <xsl:template match="Response[@Type='Bounded Length']" >
    <xsl:param name="itemNum" as="xs:integer" />
    <xsl:variable name="maxLength" as="xs:integer" select="MaxLength" />
    <xsl:element name="textArea">
      <xsl:attribute name="name" select="concat('Item', $itemNum)" />
      <xsl:attribute name="ID" select="concat('Item', $itemNum)" />
      <xsl:attribute name="class" select="'BoundedLengthTextArea'" />
      <xsl:variable name="nRows" select="ceiling($maxLength div 48.0)" />
      <xsl:if test="$nRows le 8">
        <xsl:attribute name="rows" select="$nRows" />
      </xsl:if>
      <xsl:if test="$nRows gt 8" >
        <xsl:attribute name="rows" select="'8'" />
      </xsl:if>
    </xsl:element>
  </xsl:template>

  <xsl:template match="Response[@Type='Bounded Number']" >
    <xsl:param name="itemNum" as="xs:integer" />
    <xsl:element name="input">
      <xsl:attribute name="type" select="'text'" />
      <xsl:attribute name="name" select="concat('Item', $itemNum)" />
      <xsl:attribute name="ID" select="concat('Item', $itemNum)" />
      <xsl:attribute name="class" select="'BoundedNumberInput'" />
    </xsl:element>
  </xsl:template>

  <xsl:template match="Response[@Type='Fixed Digit']" >
    <xsl:param name="itemNum" as="xs:integer" />
    <xsl:element name="input">
      <xsl:attribute name="type" select="'text'" />
      <xsl:attribute name="name" select="concat('Item', $itemNum)" />
      <xsl:attribute name="ID" select="concat('Item', $itemNum)" />
      <xsl:attribute name="class" select="'BoundedFixedDigitInput'" />
    </xsl:element>
  </xsl:template>


  <xsl:template name="writeRadioButton" >
    <xsl:param name="radioGroup" />
    <xsl:param name="radioValue" />
    <xsl:param name="radioLabel" />
    <xsl:element name="tr">
      <xsl:element name="td">
        <xsl:attribute name="class" select="'RadioInputCell'" />
        <xsl:element name="input">
          <xsl:attribute name="class" select="'RadioInput'" />
          <xsl:attribute name="type" select="'radio'" />
          <xsl:attribute name="name" select="$radioGroup" />
          <xsl:attribute name="value" select="$radioValue" />
        </xsl:element>
      </xsl:element>
      <xsl:element name="td">
        <xsl:attribute name="width" select="'100%'" />
        <xsl:element name="p">
          <xsl:attribute name="class" select="'RadioLabelParagraph'" />
          <xsl:value-of select='$radioLabel' />
        </xsl:element>
      </xsl:element>
    </xsl:element>
  </xsl:template>

</xsl:stylesheet>