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


  <xsl:output method="xml" encoding="utf-8" indent="yes"/>

  <xsl:variable name="alphabet">
    <xsl:value-of select="'ABCDEFGHIJKLMNOPQRSTUVWXYZ'"/>
  </xsl:variable>

  <xsl:variable name="tableOffset">
    <mine:x>11</mine:x>
    <mine:y>5</mine:y>
  </xsl:variable>

  <xsl:template match="/ResultDocument">
    <xsl:variable name="rd" select="."/>
    <xsl:for-each select="distinct-values(//IATResponse/ItemNum)">
      <xsl:sort select="xs:integer(.)" order="ascending"/>
      <xsl:variable name="currItemNum" select="xs:integer(.)"/>
      <xsl:element name="worksheet">
        <xsl:namespace name="r"
                       select="'http://schemas.openxmlformats.org/officeDocument/2006/relationships'"/>
        <xsl:namespace name="mc"
                       select="'http://schemas.openxmlformats.org/markup-compatibility/2006'"/>
        <xsl:namespace name="x14ac"
                       select="'http://schemas.microsoft.com/office/spreadsheetml/2009/9/ac'"/>
        <xsl:namespace name="xsl" select="'http://www.w3.org/1999/XSL/Transform'"/>
        <xsl:attribute name="mc:Ignorable" select="'x14ac'"/>
        <xsl:attribute name="ItemAnalysisWorksheetNum" select="."/>
        <xsl:variable name="dataDim">
          <mine:width>
            <xsl:value-of select="max(for $n in $rd/TestResult[some $in in IATResult/IATResponse/ItemNum satisfies xs:integer($in) eq $currItemNum] return count($n/IATResult/IATResponse[xs:integer(ItemNum) eq $currItemNum])) + 2"/>
          </mine:width>
          <mine:height>
            <xsl:value-of select="count($rd/TestResult) + 1"/>
          </mine:height>
        </xsl:variable>
        <xsl:variable name="gridspan">
          <xsl:element name="mine:MinRow">
            <xsl:value-of select="$tableOffset/mine:y"/>
          </xsl:element>
          <xsl:element name="mine:MaxRow">
            <xsl:value-of select="$tableOffset/mine:y + $dataDim/mine:height"/>
          </xsl:element>
          <xsl:element name="mine:MinCol">
            <xsl:value-of select="substring($alphabet, $tableOffset/mine:x, 1)"/>
          </xsl:element>
          <xsl:element name="mine:MaxCol">
            <xsl:variable name="maxColNum"
                          select="xs:integer($tableOffset/mine:x) + xs:integer($dataDim/mine:width)"/>
            <xsl:if test="$maxColNum lt string-length($alphabet)">
              <xsl:value-of select="substring($alphabet, $maxColNum, 1)"/>
              <xsl:if test="$maxColNum ge string-length($alphabet)">
                <xsl:value-of select="concat(substring($alphabet, $maxColNum div string-length($alphabet), 1), substring($alphabet, $maxColNum mod string-length($alphabet), 1))"/>
              </xsl:if>
            </xsl:if>
          </xsl:element>
        </xsl:variable>
        <xsl:element name="dimension">
          <xsl:attribute name="ref"
                         select="concat($gridspan/mine:MinCol, $gridspan/mine:MinRow, ':', $gridspan/mine:MaxCol, $gridspan/mine:MaxRow)"/>
        </xsl:element>
        <sheetViews>
          <sheetView tabSelected="1" workbookViewId="0">
            <selection activeCell="A1" sqref="A1"/>
          </sheetView>
        </sheetViews>
        <sheetFormatPr defaultRowHeight="15" x14ac:dyDescent="0.25"/>
        <cols>
          <col min="11" max="11" width="12.7109375" customWidth="1"/>
          <col min="12" max="12" width="15.7109375" customWidth="1"/>
        </cols>
        <xsl:element name="sheetData">
          <row r="5" spans="11:17" x14ac:dyDescent="0.25">
            <c r="K5" s="1" t="s">
              <v>1</v>
            </c>
            <c r="L5" s="1" t="s">
              <v>0</v>
            </c>
            <c r="M5" s="2" t="s">
              <v>2</v>
            </c>
            <c r="N5" s="2"/>
            <c r="O5" s="2"/>
            <c r="P5" s="2"/>
            <c r="Q5" s="2"/>
          </row>
          <xsl:for-each select="$rd/TestResult[some $n in IATResult/IATResponse/ItemNum satisifies xs:integer(n) eq $currItemNum]">
            <xsl:call-template name="OutputTesteeRow">
              <xsl:with-param name="IATResultSet" select="IATResult" />
              <xsl:with-param name="TesteeNum" select="count(preceding::TestResult) + 1" />
              <xsl:with-param name="RowNum" select="count(preceding::TestResult) + 1 + xs:integer($tableOffset/mine:y)" />
              <xsl:with-param name="span" select="concat($tableOffset/mine:x, ':', xs:integer($tableOffset/mine:x) + xs:integer($dataDim/mine:width))" />
            </xsl:call-template>
          </xsl:for-each>
        </xsl:element>
        <mergeCells count="1">
          <mergeCell ref="M5:Q5"/>
        </mergeCells>
        <pageMargins left="0.7" right="0.7" top="0.75" bottom="0.75" header="0.3" footer="0.3"/>
        <pageSetup orientation="portrait" r:id="rId1"/>
        <xsl:element name="drawing">
          <xsl:attribute namespace="r" name="id" select="concat('rId', $currItemNum + 1)" />
        </xsl:element>
      </xsl:element>
    </xsl:for-each>
  </xsl:template>

  <xsl:template name="OutputTesteeRow">
    <xsl:param name="IATResultSet" />
    <xsl:param name="TesteeNum" />
    <xsl:param name="RowNum" />
    <xsl:param name="spans" />
    <xsl:element name="row">
      <xsl:attribute name="r" select="$RowNum" />
      <xsl:attribute name="spans" select="$spans" />
      <xsl:attribute namespace="x14ac" name="dyDescent" select="0.25" />

      <xsl:for-each select="$rd/TestResult">
        <xsl:variable name="resultNum" select="position()"/>
        <xsl:variable name="testResultNode" select="."/>
        <xsl:element name="row">
          <xsl:attribute name="r" select="position()"/>
          <xsl:attribute name="spans" select="concat('1:', $dataDim/mine:width)"/>
          <xsl:attribute name="x14ac:dyDescent" select="'0.25'"/>
          <xsl:apply-templates select="SurveyResults[count(following-sibling::IATResult) gt 0]/Answer">
            <xsl:with-param name="rowNum" select="$resultNum"/>
          </xsl:apply-templates>
          <xsl:apply-templates select="IATResult">
            <xsl:with-param name="rowNum" select="$resultNum"/>
          </xsl:apply-templates>
          <xsl:apply-templates select="SurveyResults[count(preceding-sibling::IATResult) gt 0]/Answer">
            <xsl:with-param name="rowNum" select="$resultNum"/>
          </xsl:apply-templates>
        </xsl:element>
      </xsl:for-each>
    </xsl:element>
    <pageMargins left="0.7" right="0.7" top="0.75" bottom="0.75" header="0.3" footer="0.3"/>
    <pageSetup orientation="portrait" r:id="rId1"/>
    </xsl:element>
    </xsl:for-each>
  </xsl:template>

  <xsl:template match="Answer">
    <xsl:param name="rowNum"/>
    <xsl:variable name="colNum"
                  select="count(ancestor::TestResult/descendant::Answer intersect preceding::Answer) + count(ancestor::TestResult/descendant::IATResult intersect preceding::IATResult) + 1"/>
    <xsl:variable name="testElemNum" select="preceding-sibling::TestElement"/>
    <xsl:element name="c">
      <xsl:variable name="col">
        <xsl:if test="xs:integer($colNum) le string-length($alphabet)">
          <xsl:value-of select="substring($alphabet, xs:integer($colNum), 1)"/>
        </xsl:if>
        <xsl:if test="xs:integer($colNum) gt string-length($alphabet)">
          <xsl:value-of select="concat(substring($alphabet, xs:integer($colNum) div string-length($alphabet), 1), substring($alphabet, xs:integer($colNum) mod string-length($alphabet), 1))"/>
        </xsl:if>
      </xsl:variable>
      <xsl:attribute name="r" select="concat($col, $rowNum)"/>
      <xsl:variable name="respType"
                   select="//SurveyFormat[@ElementNum eq $testElemNum]/Questions[@ResponseType ne 'None'][xs:integer($colNum)]/@ResponseType"/>
      <xsl:variable name="numRespTypes"
                   select="tokenize('Likert WeightedMultiple BoundedNum', ' ')"/>
      <xsl:if test="empty(distinct-values($numRespTypes[.=$respType]))">
        <xsl:attribute name="s" select="'1'"/>
      </xsl:if>
      <xsl:element name="v">
        <xsl:value-of select="."/>
      </xsl:element>
    </xsl:element>
  </xsl:template>

  <xsl:template match="IATResult">
    <xsl:param name="rowNum"/>
    <xsl:variable name="colNum"
                  select="count(ancestor::TestResult/descendant::Answer intersect preceding::Answer) + 1"/>
    <xsl:variable name="col">
      <xsl:if test="xs:integer($colNum) le string-length($alphabet)">
        <xsl:value-of select="substring($alphabet, xs:integer($colNum), 1)"/>
      </xsl:if>
      <xsl:if test="xs:integer($colNum) gt string-length($alphabet)">
        <xsl:value-of select="concat(substring($alphabet, xs:integer($colNum) div string-length($alphabet), 1), substring($alphabet, xs:integer($colNum) mod string-length($alphabet), 1))"/>
      </xsl:if>
    </xsl:variable>
    <xsl:element name="c">
      <xsl:attribute name="r" select="concat($col, $rowNum)"/>
      <xsl:if test="IATScore eq 'NaN'">
        <xsl:attribute name="s" select="'1'"/>
        <xsl:element name="v">
          <xsl:value-of select="'Unscored'"/>
        </xsl:element>
      </xsl:if>
      <xsl:if test="IATScore ne 'NaN'">
        <xsl:element name="v">
          <xsl:value-of select="IATScore"/>
        </xsl:element>
      </xsl:if>
    </xsl:element>
  </xsl:template>
</xsl:stylesheet>