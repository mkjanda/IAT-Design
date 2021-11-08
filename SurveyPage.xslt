<?xml version="1.0" encoding="UTF-8"?>
<xsl:stylesheet xmlns:xs="http://www.w3.org/2001/XMLSchema"
								xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
								xmlns:mine="http://www.iatsoftware.net"
								version="2.0"
								exclude-result-prefixes="xs mine">

	<xsl:output method="xml" omit-xml-declaration="yes" encoding="UTF-8"
							 indent="yes" />

	<xsl:variable name="root" select="/" />

	<xsl:function name="mine:textWidth">
		<xsl:param name="numChars" />
		<xsl:param name="format" />
		<xsl:variable name="fontSize" select="substring($format/FontSize, 0, string-length($format/FontSize) - 1)" />
		<xsl:value-of select="xs:integer(ceiling(xs:integer($numChars) * xs:integer($fontSize) * 4 div 7))" />
	</xsl:function>

	<xsl:template match="Survey">
		<xsl:variable name="survey" select="." />
		<xsl:element name="html">
			<head>
				<title>
					<xsl:variable name="caption" select="./Caption"/>
					<xsl:if test="count($caption) eq 1">
						<xsl:value-of select="./Caption/Text"/>
					</xsl:if>
					<xsl:if test="count($caption) eq 0">
						<xsl:value-of select="@IAT"/>
					</xsl:if>
				</title>
				<style type="text/css">
					<xsl:for-each select="$root//CustomFont[every $s in following::CustomFont/FontFileName satisfies FontFileName ne $s]">
						<xsl:value-of select="'&#x0A;@font-face {&#x0A;'" />
						<xsl:value-of select="concat('font-family: ', ./FontFaceName, ';&#x0A;')" />
						<xsl:value-of select="concat('src: url(', $root//ServerPath, '/', ./FontFileName, ');&#x0A;')" />
						<xsl:value-of select="'}&#x0A;'" />
					</xsl:for-each>
					body {
					font: 100% Verdana, Arial, Helvetica, sans-serif;
					background: #FFFFFF;
					margin: 0;
					padding: 0;
					text-align: center;
					color: #000000;
					}

					.oneColFixCtrHdr #container {
					width: 800px;
					background: #FFFFFF;
					margin: 0 auto;
					text-align: left;
					}

					<xsl:if test="count(./Caption) eq 1">
						.oneColFixCtrHdr #header {
						background: #<xsl:value-of select="concat(./Caption/BackColorR, ./Caption/BackColorG, ./Caption/BackColorB)"/>;
						padding: 0 10px 0 20px;
						margin-bottom: 30px;
						border-bottom: <xsl:value-of select="./Caption/BorderWidth"/>px solid #<xsl:value-of select="concat(./Caption/BorderColorR, ./Caption/BorderColorG, ./Caption/BorderColorB)"/>;
						}


						.oneColFixCtrHdr #header h1 {
						margin: 0;
						padding: 10px 0;
						text-align: center;
						font-family: "Times New Roman", Times, serif;
						font-size: <xsl:value-of select="./Caption/FontSize"/>px;
						font-weight: bold;
						letter-spacing: 1px;
						color: #<xsl:value-of select="concat(./Caption/FontColorR, ./Caption/FontColorG, ./Caption/FontColorB)"/>;
						}
					</xsl:if>
					<xsl:text>

					.oneColFixCtrHdr #mainContent {
					padding: 0 20px; 
					background: #FFFFFF;
					}

					.oneColFixCtrHdr #mainContent ul {
					list-style: none;
					}

					.oneColFixCtrHdr #mainContent #ErrorsExistMsgDiv h3
					{
					font-family: "Times New Roman", Times, serif;
					font-size: 18px;
					color: #006644;
					font-weight: bold;
					}

					.oneColFixCtrHdr #mainContent ul li.ItemOdd {
					padding: 5px 5px 10px 5px;
					width: 100%;
					background: #FFFFFF;
					}

					.oneColFixCtrHdr #mainContent ul li.ItemEven {
					padding: 5px 5px 10px 5px;
					width: 100%;
					background: #DDDDDD;
					}


					.oneColFixCtrHdr #mainContent ul li.Error {
					font-family: "Times New Roman", Times, serif;
					font-size: 16px;
					color: #dd0000;
					font-style: italic;
					font-weight: normal;
					margin: 5px 0px 10px 0px;
					}


					.RadioInputCell {
					vertical-align: middle;
					padding: 0px;
					width: 25px;
					}
					</xsl:text>
					<xsl:for-each select="$root//SurveyItem">
						<xsl:value-of select="concat('h3#itemText', count(preceding-sibling::SurveyItem) + 1, ' {&#x0A;')" />
						<xsl:if test="Response/@Type ne 'Instruction'">
							<xsl:value-of select="'text-indent: -15px;&#x0A;'" />
							<xsl:value-of select="'margin: 2px 5px 3px 20px;&#x0A;'" />
						</xsl:if>
						<xsl:if test="Response/@Type eq 'Instruction'">
							<xsl:value-of select="'text-indent: 35px;&#x0A;'" />
							<xsl:if test="count(following-sibling::SurveyItem) ge 1">
								<xsl:if test="following-sibling::SurveyItem[position() eq 1]/Response/@Type ne 'Instruction'">
									<xsl:value-of select="'margin: 2px 5px 18px 20px;&#x0A;'" />
								</xsl:if>
								<xsl:if test="following-sibling::SurveyItem[position() eq 1]/Response/@Type eq 'Instruction'">
									<xsl:value-of select="'margin: 2px 5px 8px 20px;&#x0A;'" />
								</xsl:if>
							</xsl:if>
							<xsl:if test="count(following-sibling::SurveyItem) eq 0">
								<xsl:value-of select="'margin: 2px 5px 18px 20px;&#x0A;'" />
							</xsl:if>
						</xsl:if>
						<xsl:text>
							padding-left: 15px;
						</xsl:text>
						<xsl:call-template name="writeFormatCSS">
							<xsl:with-param name="format" select="SurveyItemFormat" />
						</xsl:call-template>
						<xsl:value-of select="'}&#x0A;'" />
					</xsl:for-each>
					<xsl:variable name="selectionBasedResponses">
						<xsl:sequence select="tokenize('Multiple Choice,Weighted Multiple Choice,Boolean,Likert,Multiple Selection', ',')" />
					</xsl:variable>
					<xsl:for-each select="$root//Response[contains($selectionBasedResponses, @Type)]">
						<xsl:value-of select="concat('p.response', count(../preceding-sibling::SurveyItem[Response/@Type ne 'Instruction']) + 1, '{&#x0A;')" />
						<xsl:call-template name="writeFormatCSS">
							<xsl:with-param name="format" select="./SurveyItemFormat" />
						</xsl:call-template>
						<xsl:value-of select="'padding: 0px;&#x0A;'" />
						<xsl:value-of select="'vertical-align: middle;&#x0A;'" />
						<xsl:value-of select="'text-align: left;'&#x0A;" />
						<xsl:value-of select="'}&#x0A;'" />
					</xsl:for-each>
					<xsl:text>
					.ErrorMessageDiv h3 {
					font-family: "Times New Roman", Times, serif;
					font-size: 16px;
					color: #dd0000;
					font-style: italic;
					font-weight: normal;
					margin: 2px 5px 3px 10px;
					}

					.SurveyItemDiv .RadioButtonTable {
					margin: 2px 5px 3px 40px;
					padding: 0px;
					}

					.SurveyItemDiv .CheckBoxTable {
					margin-left: 20px;
					}




					.Clear
					{
					clear: both;
					min-height: 1px;
					height: 1px;
					}

					.DateInputLabel
					{
					text-indent: -15px;
					margin: 5px 5px 5px 215px;
					padding: 5px 5px 5px 5px;
					}

					#SubmitButtonDiv
					{
					width: 100%;
					text-align: center;
					}

					#SubmitButton {
					width: 100px;
					margin-top: 20px;
					margin-left: auto;
					margin-right: auto;
					margin-bottom: 20px;
					}

					.AjaxErrorDiv {
					text-align: left;
					width: 980px;
					color: #CCCCCC;
					margin-top: 20px;
					margin-left: auto;
					margin-right: auto;
					}

					.AjaxErrorMsg {
					text-align: center;
					font-family: Arial, Helvetica, sans-serif;
					color: #000000;
					font-size: 32px;
					}

					.AjaxErrorDetail {
					font-family: "Times New Roman", Times, serif;
					font-size: 18px;
					color: #000000;
					}
				</xsl:text>
					<xsl:variable name="inputResponses">
						<xsl:sequence select="tokenize('Bounded Length,Bounded Number,Date,Fixed Digit,Regular Expression', ',')" />
					</xsl:variable>
					<xsl:for-each select="$root//SurveyItem[contains($inputResponses, Response/@Type)]/Response">
						<xsl:variable name="questionNum" select="count(../preceding-sibling::SurveyItem/Response[@Type ne 'Instruction']) + 1" />
						<xsl:variable name="responseFormat" select="SurveyItemFormat" />
						<xsl:variable name="fontSize" select="substring($responseFormat/FontSize, 0, string-length($responseFormat/FontSize) - 1)" />
						<xsl:variable name="typeSpecifics">
							<xsl:choose>
								<xsl:when test="@Type eq 'Bounded Length'">
									<MaxTextLength>
										<xsl:value-of select="mine:textWidth(MaxLength, $responseFormat)" />
									</MaxTextLength>
								</xsl:when>
								<xsl:when test="@Type eq 'Bounded Number'">
									<MaxTextLength>
										<xsl:if test="string-length(MinValue) gt string-length(MaxValue)">
											<xsl:value-of select="mine:textWidth(string-length(MinValue) - 1, $responseFormat)" />
										</xsl:if>
										<xsl:if test="string-length(MinValue) le string-length(MaxValue)">
											<xsl:value-of select="mine:textWidth(string-length(MaxValue), $responseFormat)" />
										</xsl:if>
									</MaxTextLength>
								</xsl:when>
								<xsl:when test="@Type eq 'Date'">
									<MaxTextLength>
										<xsl:value-of select="mine:textWidth(10, $responseFormat)" />
									</MaxTextLength>
								</xsl:when>
								<xsl:when test="@Type eq 'Fixed Digit'">
									<MaxTextLength>
										<xsl:value-of select="mine:textWidth(NumDigs, $responseFormat)" />
									</MaxTextLength>
								</xsl:when>
								<xsl:when test="@Type eq 'Regular Expression'">
									<MaxTextLength>270</MaxTextLength>
								</xsl:when>
							</xsl:choose>
						</xsl:variable>
						<xsl:variable name="width">
							<xsl:choose>
								<xsl:when test="xs:integer($typeSpecifics/MaxTextLength) lt 150">
									<xsl:value-of select="150" />
								</xsl:when>
								<xsl:when test="xs:integer($typeSpecifics/MaxTextLength) gt 600">
									<xsl:value-of select="600" />
								</xsl:when>
								<xsl:otherwise>
									<xsl:value-of select="xs:integer($typeSpecifics/MaxTextLength)" />
								</xsl:otherwise>
							</xsl:choose>
						</xsl:variable>
						<xsl:variable name="nameless">
							<xsl:if test="(xs:integer($typeSpecifics/MaxTextLength) gt 600) and (@Type eq 'Bounded Length')">
								<Height>
									<xsl:value-of select="ceiling((xs:integer($typeSpecifics/MaxTextLength) div $width)) * (xs:integer($fontSize) + 10)" />
								</Height>
								<Selector>
									<xsl:value-of select="concat('textarea#Item', $questionNum, ' {&#x0A;')" />
								</Selector>
							</xsl:if>
							<xsl:if test="(xs:integer($typeSpecifics/MaxTextLength) le 600) or (Response/@Type ne 'Bounded Length')">
								<Height>
									<xsl:value-of select="xs:integer($fontSize) + 6" />
								</Height>
								<Selector>
									<xsl:value-of select="concat('input#Item', $questionNum, ' {&#x0A;')" />
								</Selector>
							</xsl:if>
						</xsl:variable>
						<xsl:value-of select="$nameless/Selector" />
						<xsl:if test="Response/@Type eq 'Date'">
							<xsl:value-of select="'float: left;&#x0A;'" />
						</xsl:if>
						<xsl:value-of select="'margin: 5px 5px 5px 35px;'" />
						<xsl:value-of select="concat('width: ', $width, 'px;&#x0A;')" />
						<xsl:value-of select="concat('height: ', $nameless/Height, 'px;&#x0A;')" />
						<xsl:call-template name="writeFormatCSS">
							<xsl:with-param name="format" select="$responseFormat" />
						</xsl:call-template>
						<xsl:value-of select="'}&#x0A;'" />
					</xsl:for-each>
				</style>
				<xsl:variable name="urlParts">
					<urlPart>http://</urlPart>
					<urlPart>
						<xsl:value-of select="./ServerDomain"/>
					</urlPart>
					<xsl:if test="./ServerPort eq '80'">
						<urlPart>
							<xsl:value-of select="concat(':', ./ServerPort)" />
						</urlPart>
					</xsl:if>
					<urlPart>
						<xsl:value-of select="./ServerPath"/>
					</urlPart>
				</xsl:variable>
				<xsl:variable name="url">
					<xsl:value-of select="concat(string-join($urlParts/urlPart, ''), '/')" />
				</xsl:variable>
				<xsl:element name="script">
					<xsl:attribute name="charset" select="'UTF-8'" />
					<xsl:attribute name="type" select="'text/javascript'"/>
					<xsl:attribute name="src" select="concat($url, 'core_aes.js')"/>
					<xsl:value-of select="' '"/>
				</xsl:element>
				<xsl:element name="script">
					<xsl:attribute name="charset" select="'UTF-8'" />
					<xsl:attribute name="type" select="'text/javascript'"/>
					<xsl:attribute name="src" select="concat($url, 'MiscUtils.js')"/>
					<xsl:value-of select="' '"/>
				</xsl:element>
				<xsl:element name="script">
					<xsl:attribute name="charset" select="'UTF-8'" />
					<xsl:attribute name="type" select="'text/javascript'" />
					<xsl:attribute name="src" select="concat($url, 'SubFunct.js')"/>
					<xsl:value-of select="' '"/>
				</xsl:element>
				<xsl:element name="script">
					<xsl:attribute name="charset" select="'UTF-8'" />
					<xsl:attribute name="type" select="'text/javascript'" />
					<xsl:attribute name="src" select="concat($url, 'AjaxCall.js')" />
					<xsl:value-of select="' '"/>
				</xsl:element>
				<xsl:element name="script">
					<xsl:attribute name="charset" select="'UTF-8'" />
					<xsl:attribute name="type" select="'text/javascript'" />
					<xsl:attribute name="src" select="concat($url, @ClientID, '/', @IAT, '/', replace(@SurveyName, '[^A-Za-z0-9_]', ''), '.js?', $root//UploadTimeMillis)" />
					<xsl:value-of select="' '"/>
				</xsl:element>
			</head>
			<body class="oneColFixCtrHdr" onload="OnLoad()" onunload="OnUnload()">
				<xsl:if test="count(./Caption) eq 1">
					<div id="header">
						<h1>
							<xsl:value-of select="./Caption/Text"/>
						</h1>
					</div>
				</xsl:if>
				<div id="container">
					<div id="mainContent">
						<xsl:element name="form">
							<xsl:attribute name="id" select="'SurveyForm'"/>
							<xsl:attribute name="method" select="'post'"/>
							<xsl:element name="input">
								<xsl:attribute name="type" select="'hidden'"/>
								<xsl:attribute name="name" select="'AdministeredItem'"/>
								<xsl:attribute name="value" select="./@SurveyName"/>
							</xsl:element>
							<xsl:element name="input">
								<xsl:attribute name="type" select="'hidden'" />
								<xsl:attribute name="id" select="'k'" />
							</xsl:element>
							<xsl:element name="input">
								<xsl:attribute name="type" select="'hidden'" />
								<xsl:attribute name="id" select="'ek'" />
							</xsl:element>
							<xsl:element name="input">
								<xsl:attribute name="type" select="'hidden'" />
								<xsl:attribute name="id" select="'hexLen'" />
							</xsl:element>
							<xsl:element name="input">
								<xsl:attribute name="type" select="'hidden'" />
								<xsl:attribute name="id" select="'hexLine1'" />
							</xsl:element>
							<xsl:element name="input">
								<xsl:attribute name="type" select="'hidden'" />
								<xsl:attribute name="id" select="'hexLine2'" />
							</xsl:element>
							<xsl:element name="input">
								<xsl:attribute name="type" select="'hidden'" />
								<xsl:attribute name="id" select="'hexLine3'" />
							</xsl:element>
							<xsl:element name="input">
								<xsl:attribute name="type" select="'hidden'" />
								<xsl:attribute name="id" select="'hexLine4'" />
							</xsl:element>
							<xsl:element name="input">
								<xsl:attribute name="type" select="'hidden'" />
								<xsl:attribute name="name" select="'NumItems'" />
								<xsl:attribute name="value" select="count(//Response[@Type ne 'Instruction'])" />
							</xsl:element>
							<xsl:element name="ul">
								<xsl:attribute name="id" select="'QuestionList'"/>
								<xsl:apply-templates select="SurveyItem"/>
							</xsl:element>
							<xsl:element name="div">
								<xsl:attribute name="id" select="'ErrorsExistMsgDiv'"/>
								<xsl:value-of select="' '"/>
							</xsl:element>
							<xsl:element name="div">
								<xsl:attribute name="id" select="'SubmitButtonDiv'"/>
								<xsl:element name="input">
									<xsl:attribute name="id" select="'SubmitButton'"/>
									<xsl:attribute name="type" select="'submit'"/>
									<xsl:attribute name="value" select="'Submit'"/>
								</xsl:element>
							</xsl:element>
						</xsl:element>
					</div>
				</div>
			</body>
		</xsl:element>

	</xsl:template>

	<xsl:template match="SurveyItem">
		<xsl:variable name="precedingNodes" select="preceding-sibling::node()"/>
		<xsl:variable name="precedingSurveyItems"
									select="$precedingNodes[compare(name(), 'SurveyItem') eq 0]"/>
		<xsl:variable name="questionNum"
									select="count($precedingSurveyItems/Response[@Type ne 'Instruction']) + 1"/>
		<xsl:variable name="itemNum" select="count($precedingSurveyItems) + 1" />
		<xsl:element name="li">
			<xsl:attribute name="id" select="concat('ItemLITag', $itemNum)"/>
			<xsl:if test="Response/@Type ne 'Instruction'">
				<xsl:choose>
					<xsl:when test="(xs:integer($questionNum) mod 2) eq 0">
						<xsl:attribute name="class" select="'ItemEven'"/>
					</xsl:when>
					<xsl:when test="(xs:integer($questionNum) mod 2) eq 1">
						<xsl:attribute name="class" select="'ItemOdd'"/>
					</xsl:when>
				</xsl:choose>
			</xsl:if>
			<xsl:element name="div">
				<xsl:if test="Response/@Type eq 'Instruction'">
					<xsl:attribute name="class" select="'InstructionsDiv'"/>
				</xsl:if>
				<xsl:if test="Response/@Type ne 'Instruction'">
					<xsl:attribute name="class" select="'SurveyItemDiv'"/>
				</xsl:if>
				<xsl:element name="h3">
					<xsl:attribute name="id" select="concat('itemText', $itemNum)" />
					<xsl:value-of select="Text"/>
				</xsl:element>
				<xsl:apply-templates select="Response">
					<xsl:with-param name="itemNum" as="xs:integer" select="$questionNum"/>
				</xsl:apply-templates>
			</xsl:element>
		</xsl:element>
	</xsl:template>

	<xsl:template match="Response[@Type='Likert']">
		<xsl:param name="itemNum" as="xs:integer"/>
		<xsl:variable name="reverseScored" select="./IsReverseScored"/>
		<xsl:variable name="numChoices" select="xs:integer(./NumChoices)"/>
		<xsl:element name="table">
			<xsl:attribute name="width" select="'90%'"/>
			<xsl:attribute name="class" select="'RadioButtonTable'"/>
			<xsl:for-each select="./ChoiceDescriptions/Choice">
				<xsl:call-template name="writeRadioButton">
					<xsl:with-param name="itemNum" select="$itemNum"/>
					<xsl:with-param name="radioValue">
						<xsl:if test="$reverseScored = &#34;True&#34;">
							<xsl:value-of select="$numChoices + 1 - position()"/>
						</xsl:if>
						<xsl:if test="$reverseScored = &#34;False&#34;">
							<xsl:value-of select="position()"/>
						</xsl:if>
					</xsl:with-param>
					<xsl:with-param name="radioLabel" select="."/>
				</xsl:call-template>
			</xsl:for-each>
		</xsl:element>
	</xsl:template>

	<xsl:template match="Response[@Type='Boolean']">
		<xsl:param name="itemNum" as="xs:integer"/>
		<xsl:element name="table">
			<xsl:attribute name="width" select="'90%'"/>
			<xsl:attribute name="class" select="'RadioButtonTable'"/>
			<xsl:call-template name="writeRadioButton">
				<xsl:with-param name="itemNum" select="$itemNum"/>
				<xsl:with-param name="radioValue" select="'1'"/>
				<xsl:with-param name="radioLabel" select="./TrueStatement"/>
			</xsl:call-template>
			<xsl:call-template name="writeRadioButton">
				<xsl:with-param name="itemNum" select="$itemNum"/>
				<xsl:with-param name="radioValue" select="'0'"/>
				<xsl:with-param name="radioLabel" select="./FalseStatement"/>
			</xsl:call-template>
		</xsl:element>
	</xsl:template>

	<xsl:template match="Response[@Type='Multiple Choice']">
		<xsl:param name="itemNum" as="xs:integer"/>
		<xsl:element name="table">
			<xsl:attribute name="width" select="'90%'"/>
			<xsl:attribute name="class" select="'RadioButtonTable'"/>
			<xsl:for-each select="./Choices/Choice">
				<xsl:call-template name="writeRadioButton">
					<xsl:with-param name="itemNum" select="$itemNum"/>
					<xsl:with-param name="radioValue" select="position()"/>
					<xsl:with-param name="radioLabel" select="."/>
				</xsl:call-template>
			</xsl:for-each>
		</xsl:element>
	</xsl:template>

	<xsl:template match="Response[@Type='Weighted Multiple Choice']">
		<xsl:param name="itemNum" as="xs:integer"/>
		<xsl:element name="table">
			<xsl:attribute name="width" select="'90%'"/>
			<xsl:attribute name="class" select="'RadioButtonTable'"/>
			<xsl:for-each select="./WeightedChoices/WeightedChoice">
				<xsl:call-template name="writeRadioButton">
					<xsl:with-param name="itemNum" select="$itemNum"/>
					<xsl:with-param name="radioValue" select="./Weight"/>
					<xsl:with-param name="radioLabel" select="./Choice"/>
				</xsl:call-template>
			</xsl:for-each>
		</xsl:element>
	</xsl:template>

	<xsl:template match="Response[@Type='Multiple Selection']">
		<xsl:param name="itemNum" as="xs:integer"/>
		<xsl:variable name="numTableRows" select="xs:integer(ceiling((count(./Labels/Label)) div 2))"/>
		<xsl:variable name="labels" select="./Labels"/>
		<xsl:element name="table">
			<xsl:attribute name="width" select="'90%'"/>
			<xsl:attribute name="class" select="'CheckBoxTable'"/>
			<xsl:for-each select="1 to $numTableRows">
				<xsl:variable name="col1Index" select="position()"/>
				<xsl:variable name="col2Index" select="position() + $numTableRows"/>
				<tr>
					<td style="width: 0px;">
						<xsl:element name="input">
							<xsl:attribute name="type" select="'checkbox'"/>
							<xsl:attribute name="name" select="concat('Item', $itemNum, '_', $col1Index)"/>
							<xsl:attribute name="ID" select="concat('Item', $itemNum, '_', $col1Index)"/>
						</xsl:element>
					</td>
					<td>
						<xsl:element name="p">
							<xsl:attribute name="class" select="concat('response', $itemNum)" />
							<xsl:element name="label">
								<xsl:attribute name="for" select="concat('Item', $itemNum, '_', $col1Index)"/>
								<xsl:value-of select="$labels/Label[position() = $col1Index]"/>
							</xsl:element>
						</xsl:element>
					</td>
					<xsl:if test="position() + $numTableRows le count($labels/Label)">
						<td style="width: 0px;">
							<xsl:element name="input">
								<xsl:attribute name="type" select="'checkbox'"/>
								<xsl:attribute name="name" select="concat('Item', $itemNum, '_', $col2Index)"/>
								<xsl:attribute name="ID" select="concat('Item', $itemNum, '_', $col2Index)"/>
							</xsl:element>
						</td>
						<td>
							<xsl:element name="div">
								<xsl:element name="p">
									<xsl:attribute name="class" select="concat('response', $itemNum)" />
									<xsl:element name="label">
										<xsl:attribute name="for" select="concat('Item', $itemNum, '_', $col2Index)"/>
										<xsl:value-of select="$labels/Label[position() = $col2Index]"/>
									</xsl:element>
								</xsl:element>
							</xsl:element>
						</td>

					</xsl:if>
				</tr>
			</xsl:for-each>
		</xsl:element>
	</xsl:template>

	<xsl:template match="Response[@Type='Date']">
		<xsl:param name="itemNum" as="xs:integer"/>
		<xsl:variable name="monthNames">
			<month number="1">January</month>
			<month number="2">February</month>
			<month number="3">March</month>
			<month number="4">April</month>
			<month number="5">May</month>
			<month number="6">June</month>
			<month number="7">July</month>
			<month number="8">August</month>
			<month number="9">September</month>
			<month number="10">October</month>
			<month number="11">November</month>
			<month number="12">December</month>
		</xsl:variable>
		<xsl:element name="div">
			<xsl:attribute name="id" select="concat('response', $itemNum)" />
			<xsl:element name="input">
				<xsl:attribute name="type" select="'text'"/>
				<xsl:attribute name="name" select="concat('Item', $itemNum)"/>
				<xsl:attribute name="id" select="concat('Item', $itemNum)"/>
				<xsl:attribute name="class" select="'DateInput'"/>
			</xsl:element>
			<xsl:element name="p">
				<xsl:attribute name="class" select="DateInputLabel" />
				<xsl:attribute name="id" select="concat('DateInputLabel', $itemNum)"/>
				<xsl:if test="StartDate[@HasValue eq 'True']">
					<xsl:variable name="startMonth" select="StartDate/Month"/>
					<xsl:if test="EndDate[@HasValue eq 'True']">
						<xsl:variable name="endMonth" select="EndDate/Month"/>
						Please enter a date between <xsl:value-of select="concat($monthNames/month[@number eq $startMonth], ' ')"/>
						<xsl:value-of select="StartDate/Day"/>, <xsl:value-of select="StartDate/Year"/> and
						<xsl:value-of select="concat($monthNames/month[@number eq $endMonth], ' ')"/>
						<xsl:value-of select="EndDate/Day"/>,
						<xsl:value-of select="EndDate/Year"/> in MM/DD/YYYY format.
					</xsl:if>
					<xsl:if test="EndDate[@HasValue ne 'True']">
						Please enter a date after <xsl:value-of select="concat($monthNames/month[@number eq $startMonth], ' ')"/>
						<xsl:value-of select="StartDate/Day"/>, <xsl:value-of select="StartDate/Year"/>
						in MM/DD/YYYY format.
					</xsl:if>
				</xsl:if>
				<xsl:if test="StartDate[@HasValue ne 'True']">
					<xsl:if test="EndDate[@HasValue eq 'True']">
						<xsl:variable name="endMonth" select="EndDate/Month"/>
						Please enter a date before <xsl:value-of select="concat($monthNames/month[@number eq $endMonth], ' ')"/>
						<xsl:value-of select="EndDate/Day"/>, <xsl:value-of select="EndDate/Year"/>
						in MM/DD/YYYY format.
					</xsl:if>
					<xsl:if test="EndDate[@HasValue ne 'True']">
						Please enter a date in MM/DD/YYYY format.
					</xsl:if>
				</xsl:if>
			</xsl:element>
		</xsl:element>
		<br class="Clear"/>
	</xsl:template>

	<xsl:template match="Response[@Type='Bounded Length']">
		<xsl:param name="itemNum" as="xs:integer"/>
		<xsl:variable name="maxTextLength" as="xs:integer" select="mine:textWidth(MaxLength, SurveyItemFormat)" />
		<xsl:element name="div">
			<xsl:attribute name="id" select="concat('response', $itemNum)" />
			<xsl:if test="$maxTextLength le 600">
				<xsl:element name="input">
					<xsl:attribute name="name" select="concat('Item', $itemNum)" />
					<xsl:attribute name="id" select="concat('Item', $itemNum)" />
					<xsl:attribute name="class" select="'BoundedLengthInput'" />
					<xsl:if test="$itemNum eq xs:integer(//Survey/@UniqueResponseItem)">
						<xsl:attribute name="onblur" select="'CheckUniqueResponse()'" />
					</xsl:if>
				</xsl:element>
			</xsl:if>
			<xsl:if test="$maxTextLength gt 600">
				<xsl:element name="textarea">
					<xsl:attribute name="name" select="concat('Item', $itemNum)"/>
					<xsl:attribute name="id" select="concat('Item', $itemNum)"/>
					<xsl:attribute name="class" select="'BoundedLengthTextArea'"/>
					<xsl:variable name="nRows" select="ceiling($maxTextLength div 600)"/>
					<xsl:if test="$nRows le 8">
						<xsl:attribute name="rows" select="$nRows"/>
					</xsl:if>
					<xsl:if test="$nRows gt 8">
						<xsl:attribute name="rows" select="'8'"/>
					</xsl:if>
					<xsl:if test="$itemNum eq xs:integer(//Survey/@UniqueResponseItem)">
						<xsl:attribute name="onblur" select="'CheckUniqueResponse()'" />
					</xsl:if>
					<xsl:value-of select="' '"/>
				</xsl:element>
			</xsl:if>
		</xsl:element>
	</xsl:template>

	<xsl:template match="Response[@Type='Bounded Number']">
		<xsl:param name="itemNum" as="xs:integer"/>
		<xsl:element name="div">
			<xsl:attribute name="id" select="concat('response', $itemNum)" />
			<xsl:element name="input">
				<xsl:attribute name="type" select="'text'"/>
				<xsl:attribute name="name" select="concat('Item', $itemNum)"/>
				<xsl:attribute name="ID" select="concat('Item', $itemNum)"/>
				<xsl:attribute name="class" select="'BoundedNumberInput'"/>
			</xsl:element>
		</xsl:element>
	</xsl:template>

	<xsl:template match="Response[@Type='Fixed Digit']">
		<xsl:param name="itemNum" as="xs:integer"/>
		<xsl:element name="div">
			<xsl:attribute name="id" select="concat('response', $itemNum)" />
			<xsl:element name="input">
				<xsl:attribute name="type" select="'text'"/>
				<xsl:attribute name="name" select="concat('Item', $itemNum)"/>
				<xsl:attribute name="ID" select="concat('Item', $itemNum)"/>
				<xsl:attribute name="class" select="'FixedDigitInput'"/>
				<xsl:if test="$itemNum eq xs:integer(//Survey/@UniqueResponseItem)">
					<xsl:attribute name="onblur" select="'CheckUniqueResponse()'" />
				</xsl:if>
			</xsl:element>
		</xsl:element>
	</xsl:template>

	<xsl:template match="Response[@Type='Regular Expression']">
		<xsl:param name="itemNum" as="xs:integer"/>
		<xsl:element name="div">
			<xsl:attribute name="id" select="concat('response', $itemNum)" />
			<xsl:element name="input">
				<xsl:attribute name="type" select="'text'"/>
				<xsl:attribute name="name" select="concat('Item', $itemNum)"/>
				<xsl:attribute name="ID" select="concat('Item', $itemNum)"/>
				<xsl:attribute name="class" select="'RegExInput'"/>
				<xsl:if test="$itemNum eq xs:integer(//Survey/@UniqueResponseItem)">
					<xsl:attribute name="onblur" select="'CheckUniqueResponse()'" />
				</xsl:if>
			</xsl:element>
		</xsl:element>
	</xsl:template>

	<xsl:template name="writeRadioButton">
		<xsl:param name="itemNum"/>
		<xsl:param name="radioValue"/>
		<xsl:param name="radioLabel"/>
		<xsl:element name="tr">
			<xsl:element name="td">
				<xsl:attribute name="class" select="'RadioInputCell'"/>
				<xsl:element name="input">
					<xsl:attribute name="class" select="'RadioInput'"/>
					<xsl:attribute name="type" select="'radio'"/>
					<xsl:attribute name="name" select="concat('Item', $itemNum)"/>
					<xsl:attribute name="value" select="$radioValue"/>
				</xsl:element>
			</xsl:element>
			<xsl:element name="td">
				<xsl:element name="div">
					<xsl:element name="p">
						<xsl:attribute name="class" select="'RadioLabelParagraph'"/>
						<xsl:attribute name="class" select="concat('response', $itemNum)" />
						<xsl:value-of select="$radioLabel"/>
					</xsl:element>
				</xsl:element>
			</xsl:element>
		</xsl:element>
	</xsl:template>

	<xsl:template name="writeFormatCSS">
		<xsl:param name="format" />
		<xsl:value-of select="concat('font-size: ', $format/FontSize, ';&#x0A;')" />
		<xsl:value-of select="concat('color: #', $format/ColorR, $format/ColorG, $format/ColorB, ';&#x0A;')" />
		<xsl:if test="$format/Bold eq 'True'">
			<xsl:value-of select="'font-weight: bold;&#x0A;'" />
		</xsl:if>
		<xsl:if test="$format/Bold eq 'False'">
			<xsl:value-of select="'font-weight: normal;&#x0A;'" />
		</xsl:if>
		<xsl:if test="$format/Italic eq 'True'">
			<xsl:value-of select="'font-style: italic;&#x0A;'" />
		</xsl:if>
		<xsl:if test="$format/Italic eq 'False'">
			<xsl:value-of select="'font-style: normal;&#x0A;'" />
		</xsl:if>
		<xsl:if test="$format/Font eq 'sansSerif'">
			<xsl:value-of select="'font-family: sans-serif;&#x0A;'" />
		</xsl:if>
		<xsl:if test="$format/Font ne 'sansSerif'">
			<xsl:value-of select="concat('font-family: ', $format/Font, ';&#x0A;')" />
		</xsl:if>
	</xsl:template>
</xsl:stylesheet>