<?xml version="1.0" encoding="UTF-8"?>
<xsl:stylesheet xmlns:xs="http://www.w3.org/2001/XMLSchema"
                xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
                xmlns="http://schemas.openxmlformats.org/spreadsheetml/2006/main"
                xmlns:r="http://schemas.openxmlformats.org/officeDocument/2006/relationships"
                xmlns:mc="http://schemas.openxmlformats.org/markup-compatibility/2006"
                xmlns:x14ac="http://schemas.microsoft.com/office/spreadsheetml/2009/9/ac"
                xmlns:mine="http://www.iatsoftware.net/dummy"
                version="2.0"
                exclude-result-prefixes="xs mine">


  <xsl:output method="xml" encoding="utf-8" indent="yes" standalone="yes"/>
  <xsl:variable name="alphabet">
    <xsl:value-of select="'ABCDEFGHIJKLMNOPQRSTUVWXYZ'" />
  </xsl:variable>

  <xsl:template match="/ResultDocument">
    <xsl:element name="worksheet">
      <xsl:namespace name="r" select="'http://schemas.openxmlformats.org/officeDocument/2006/relationships'" />
      <xsl:namespace name="mc" select="'http://schemas.openxmlformats.org/markup-compatibility/2006'" />
      <xsl:namespace name="x14ac" select="'http://schemas.microsoft.com/office/spreadsheetml/2009/9/ac'" />
      <xsl:namespace name="xsl" select="'http://www.w3.org/1999/XSL/Transform'" />
      <xsl:attribute name="mc:Ignorable" select="'x14ac'" />
      <xsl:variable name="dataDim">
        <mine:width>
          <xsl:value-of select="1 + count(SurveyDesign/SurveyFormat/Questions[@ResponseType ne 'None'])" />
        </mine:width>
        <mine:height>
          <xsl:value-of select="count(TestResult)" />
        </mine:height>
      </xsl:variable>
      <xsl:element name="dimension">
        <xsl:variable name="gridspan">
          <xsl:element name="mine:MinRow">1</xsl:element>
          <xsl:element name="mine:MaxRow">
            <xsl:value-of select="$dataDim/mine:height" />
          </xsl:element>
          <xsl:element name="mine:MinCol">
            <xsl:value-of select="'A'" />
          </xsl:element>
          <xsl:element name="mine:MaxCol">
            <xsl:if test="xs:integer($dataDim/mine:width) lt string-length($alphabet)">
              <xsl:value-of select="substring($alphabet, xs:integer($dataDim/mine:width), 1)" />
              <xsl:if test="xs:integer($dataDim/mine:height) ge string-length($alphabet)">
                <xsl:value-of select="concat(substring($alphabet, xs:integer($dataDim/mine:width) div string-length($alphabet), 1), substring($alphabet, xs:integer($dataDim/mine:width) mod string-length($alphabet), 1))" />
              </xsl:if>
            </xsl:if>
          </xsl:element>
        </xsl:variable>
        <xsl:attribute name="ref" select="concat($gridspan/mine:MinCol, $gridspan/mine:MinRow, ':', $gridspan/mine:MaxCol, $gridspan/mine:MaxRow)" />
      </xsl:element>
      <sheetViews>
        <sheetView tabSelected="1" workbookViewId="0">
          <selection activeCell="A1" sqref="A1"/>
        </sheetView>
      </sheetViews>
      <sheetFormatPr defaultRowHeight="15" x14ac:dyDescent="0.25"/>
      <cols>
        <xsl:element name="col">
          <xsl:attribute name="min" select="$dataDim/mine:width" />
          <xsl:attribute name="max" select="$dataDim/mine:width" />
          <xsl:attribute name="width" select="15" />
          <xsl:attribute name="customWidth" select="1" />
        </xsl:element>
      </cols>
      <xsl:element name="sheetData">
        <xsl:for-each select="TestResult">
          <xsl:variable name="resultNum" select="position()" />
          <xsl:variable name="testResultNode" select="." />
          <xsl:element name="row">
            <xsl:attribute name="r" select="position()" />
            <xsl:attribute name="spans" select="concat('1:', $dataDim/mine:width)" />
            <xsl:attribute name="x14ac:dyDescent" select="'0.25'" />
            <xsl:apply-templates select="SurveyResults[count(following-sibling::IATResult) gt 0]/Answer">
              <xsl:with-param name="rowNum" select="$resultNum" />
            </xsl:apply-templates>
            <xsl:apply-templates select="IATResult">
              <xsl:with-param name="rowNum" select="$resultNum" />
            </xsl:apply-templates>
            <xsl:apply-templates select="SurveyResults[count(preceding-sibling::IATResult) gt 0]/Answer">
              <xsl:with-param name="rowNum" select="$resultNum" />
            </xsl:apply-templates>
          </xsl:element>
        </xsl:for-each>
      </xsl:element>
      <pageMargins left="0.7" right="0.7" top="0.75" bottom="0.75" header="0.3" footer="0.3"/>
      <pageSetup orientation="portrait" r:id="rId1"/>
    </xsl:element>
  </xsl:template>

  <xsl:template match="Answer">
    <xsl:param name="rowNum" />
    <xsl:variable name="colNum" select="count(ancestor::TestResult/descendant::Answer intersect preceding::Answer) + count(ancestor::TestResult/descendant::IATResult intersect preceding::IATResult) + 1" />
    <xsl:variable name="testElemNum" select="preceding-sibling::TestElement" />
    <xsl:element name="c">
      <xsl:variable name="col">
        <xsl:if test="xs:integer($colNum) le string-length($alphabet)">
          <xsl:value-of select="substring($alphabet, xs:integer($colNum), 1)" />
        </xsl:if>
        <xsl:if test="xs:integer($colNum) gt string-length($alphabet)">
          <xsl:value-of select="concat(substring($alphabet, xs:integer($colNum) div string-length($alphabet), 1), substring($alphabet, xs:integer($colNum) mod string-length($alphabet), 1))" />
        </xsl:if>
      </xsl:variable>
      <xsl:attribute name="r" select="concat($col, $rowNum)" />
      <xsl:variable name="respType" select="//SurveyFormat[@ElementNum eq $testElemNum]/Questions[@ResponseType ne 'None'][xs:integer($colNum)]/@ResponseType" />
      <xsl:variable name="numRespTypes" select="tokenize('Likert WeightedMultiple BoundedNum', ' ')" />
      <xsl:if test="empty(distinct-values($numRespTypes[.=$respType]))">
        <xsl:attribute name="s" select="'1'" />
      </xsl:if>
      <xsl:element name="v">
        <xsl:value-of select="." />
      </xsl:element>
    </xsl:element>
  </xsl:template>

  <xsl:template match="IATResult">
    <xsl:param name="rowNum" />
    <xsl:variable name="colNum" select="count(ancestor::TestResult/descendant::Answer intersect preceding::Answer) + 1" />
    <xsl:variable name="col">
      <xsl:if test="xs:integer($colNum) le string-length($alphabet)">
        <xsl:value-of select="substring($alphabet, xs:integer($colNum), 1)" />
      </xsl:if>
      <xsl:if test="xs:integer($colNum) gt string-length($alphabet)">
        <xsl:value-of select="concat(substring($alphabet, xs:integer($colNum) div string-length($alphabet), 1), substring($alphabet, xs:integer($colNum) mod string-length($alphabet), 1))" />
      </xsl:if>
    </xsl:variable>
    <xsl:element name="c">
      <xsl:attribute name="r" select="concat($col, $rowNum)" />
      <xsl:if test="IATScore eq 'NaN'">
        <xsl:attribute name="s" select="'1'" />
        <xsl:element name="v">
          <xsl:value-of select="'Unscored'" />
        </xsl:element>
      </xsl:if>
      <xsl:if test="IATScore ne 'NaN'">
        <xsl:element name="v">
          <xsl:value-of select="IATScore" />
        </xsl:element>
      </xsl:if>
    </xsl:element>
  </xsl:template>
</xsl:stylesheet>
