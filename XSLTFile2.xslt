<?xml version="1.0" encoding="UTF-8"?>
<xsl:stylesheet xmlns:xs="http://www.w3.org/2001/XMLSchema"
                xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
                version="2.0"
                exclude-result-prefixes="xs">

  <xsl:output method="xml" encoding="utf-8" indent="yes"/>



  <xsl:variable name="GlobalAbbreviations">
    <xsl:variable name="Globals" select="string-join(for $elem in //Declarations/Declaration return replace($elem, '^var\s+(.+);$', '$1'), ', ')" />
    <xsl:analyze-string select="$Globals" regex="([A-Za-z_][A-Za-z0-9_]*)(\s*=(\s+|[^;=/,&#x22;]+?|&#x22;[^&#x22;\n\r]*?&#x22;|\(([^;=,&#x22;]*?,?(&#x22;[^\n\r&#x22;]*?&#x22;)?)+\)|/[^/\n]+?/)+?)?,\s+">
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
            <xsl:value-of select="regex-group(2)" />
          </xsl:element>
        </xsl:element>
      </xsl:matching-substring>
    </xsl:analyze-string>
  </xsl:variable>

  <xsl:template match="CodeFile">
    <xsl:variable name="fileCode">
      <xsl:element name="Globals">
        <xsl:element name="CodeLine">
          <xsl:value-of select="'var '"/>
          <xsl:for-each select="$GlobalAbbreviations/Entry">
            <xsl:value-of select="concat(NewName, Assign)"/>
            <xsl:if test="position() eq last()">
              <xsl:value-of select="';'"/>
            </xsl:if>
            <xsl:if test="position() ne last()">
              <xsl:value-of select="', '"/>
            </xsl:if>
          </xsl:for-each>
        </xsl:element>
      </xsl:element>
      <xsl:element name="Classes">
        <xsl:for-each select="Classes/Class">
          <xsl:variable name="constructedClass">
            <xsl:call-template name="ConstructClass">
              <xsl:with-param name="class" select="."/>
              <xsl:with-param name="nameBase" select="concat('C', position())" />
            </xsl:call-template>
          </xsl:variable>
          <xsl:call-template name="WriteClassCode">
            <xsl:with-param name="class" select="$constructedClass/Class" />
          </xsl:call-template>
        </xsl:for-each>
      </xsl:element>
      <xsl:element name="Functions">
        <xsl:attribute name="NumFunctions" select="count($Functions/Function) + count($processItemFunctions//Function)" />
        <xsl:for-each select="$Functions/Function union $processItemFunctions//Function">
          <xsl:variable name="mungedCode">
            <xsl:call-template name="mungeFunction">
              <xsl:with-param name="params" select="Params/Param" />
              <xsl:with-param name="varDeclLine" select="VarDecls" />
              <xsl:with-param name="codeLines" select="Code" />
            </xsl:call-template>
          </xsl:variable>
          <xsl:variable name="subFunctList">
            <xsl:call-template name="ConstructFunction">
              <xsl:with-param name="functionName" select="concat('F', position())" />
              <xsl:with-param name="processedCode" select="$mungedCode)" />
              <xsl:with-param name="params" select="Params" />
            </xsl:call-template>
          </xsl:variable>
          <xsl:call-template name="WriteFunctionCode">
            <xsl:with-param name="functionName" select="@FunctionName" />
            <xsl:with-param name="params" select="Params" />
            <xsl:with-param name="subFuncts" select="$subFunctList" />
          </xsl:call-template>
        </xsl:for-each>
      </xsl:element>
    </xsl:variable>
    <xsl:element name="CodeFile">
      <xsl:element name="CodeLines">
        <xsl:element name="CodeLine">
          <xsl:attribute name="Name" select="'CL1'" />
          <xsl:value-of select="'var '"/>
          <xsl:for-each select="$fileCode//Function">
            <xsl:value-of select="concat(@Name, ' = new SubFunct(&quot;', @Name, '&quot;)')"/>
            <xsl:if test="position() eq last()">
              <xsl:value-of select="';'"/>
            </xsl:if>
            <xsl:if test="position() lt last()">
              <xsl:value-of select="', '"/>
            </xsl:if>
          </xsl:for-each>
        </xsl:element>
        <xsl:for-each select="$fileCode//CodeLine">
          <xsl:element name="CodeLine">
            <xsl:attribute name="Name" select="concat('CL', position() + 1)" />
            <xsl:value-of select="." />
          </xsl:element>
        </xsl:for-each>
      </xsl:element>
      <xsl:element name="Functions">
        <xsl:copy-of select="$fileCode//Function"/>
      </xsl:element>
    </xsl:element>
  </xsl:template>


  <xsl:template name="mungeFunction" >
    <xsl:param name="params" />
    <xsl:param name="varDeclLine" />
    <xsl:param name="codeLines" />
    <xsl:variable name="globals" select="$GlobalAbbreviations" />
    <xsl:variable name="varLookupTable">
      <xsl:variable name="locals">
        <xsl:analyze-string select="replace($varDeclLine, 'var\s+(.+)', '$1')" regex="([A-Za-z_][A-Za-z0-9_]*)(\s*=(\s+|[^;=/,&#x22;]+?|&#x22;[^&#x22;\n\r]*?&#x22;|\(([^;=,&#x22;]*?,?(&#x22;[^\n\r&#x22;]*?&#x22;)?)+\)|/[^/\n]+?/)+?)?,\s+">
          <xsl:matching-substring>
            <xsl:element name="Entry">
              <xsl:attribute name="type" select="'Local'" />
              <xsl:element name="OrigName">
                <xsl:value-of select="regex-group(1)" />
              </xsl:element>
              <xsl:element name="NewName">
                <xsl:value-of select="concat('_l.v', position())" />
              </xsl:element>
              <xsl:element name="Assign">
                <xsl:value-of select="regex-group(2)" />
              </xsl:element>
            </xsl:element>
          </xsl:matching-substring>
        </xsl:analyze-string>
      </xsl:variable>
      <xsl:copy-of select="$locals/Entry" />
      <xsl:for-each select="$params">
        <xsl:element name="Entry">
          <xsl:attribute name="type" select="'param'" />
          <xsl:element name="OrigName">
            <xsl:value-of select="." />
          </xsl:element>
          <xsl:element name="NewName">
            <xsl:value-of select="concat('_p[', position() - 1, ']')" />
          </xsl:element>
        </xsl:element>
      </xsl:for-each>
      <xsl:for-each select="$globals/Entry[every $le in $locals/Entry satisfies ($le/OrigName ne OrigName)]">
        <xsl:element name="Entry">
          <xsl:attribute name="type" select="'global'" />
          <xsl:element name="OrigName">
            <xsl:value-of select="OrigName" />
          </xsl:element>
          <xsl:element name="NewName">
            <xsl:value-of select="NewName" />
          </xsl:element>
        </xsl:element>
      </xsl:for-each>
    </xsl:variable>
    <xsl:for-each select="$varLookupTable/Entry[(@type eq 'local') and (string-length(Assign) gt 0)]">
      <xsl:element name="Code">
        <xsl:value-of select="concat(NewName, ' = ', Assign, ';')"/>
      </xsl:element>
    </xsl:for-each>
    <xsl:variable name="varRegEx" select="concat('(^|[^A-Za-z_\.\|])(', string-join($varLookupTable/Entry/OrigName, '|'), ')([^A-Za-z0-9_\|])')" />
    <xsl:for-each select="tokenize($codeLines, '&#x0A;')">
      <xsl:element name="Code">
        <xsl:analyze-string select="." regex="{$varRegEx}">
          <xsl:matching-substring>
            <xsl:value-of select="concat(regex-group(1), $varLookupTable/Entry[OrigName eq regex-group(2)]/NewName, regex-group(3))" />
          </xsl:matching-substring>
          <xsl:non-matching-substring>
            <xsl:value-of select="." />
          </xsl:non-matching-substring>
        </xsl:analyze-string>
      </xsl:element>
    </xsl:for-each>
  </xsl:template>

  <xsl:template name="ConstructFunction">
    <xsl:param name="functionName" />
    <xsl:param name="processedCode" />
    <xsl:param name="params" />
    <xsl:variable name="processedCode">
      <xsl:call-template name="mungeFunction">
        <xsl:with-param name="functionCode" select="$functionCode" />
        <xsl:with-param name="params" select="$params" />
        <xsl:with-param name="globals" select="$GlobalAbbreviations" />
      </xsl:call-template>
    </xsl:variable>
    <xsl:variable name="lineDelims">
      <xsl:element name="TermExpression">
        <xsl:attribute name="type" select="'Return'" />
        <xsl:attribute name="openCount" select="xs:integer(0)" />
        <xsl:text>(^|\s+)return\s*?</xsl:text>
      </xsl:element>
      <xsl:element name="TermExpression">
        <xsl:attribute name="type" select="'OpenBrace'" />
        <xsl:attribute name="openCount" select="xs:integer(1)" />
        <xsl:text>\{</xsl:text>
      </xsl:element>
      <xsl:element name="TermExpression">
        <xsl:attribute name="type" select="'TerminatingParen'" />
        <xsl:attribute name="openCount" select="xs:integer(0)" />
        <xsl:text>\)\s*$</xsl:text>
      </xsl:element>
      <xsl:element name="TermExpression">
        <xsl:attribute name="type" select="'CloseBrace'" />
        <xsl:attribute name="openCount" select="xs:integer(-1)" />
        <xsl:text>\}</xsl:text>
      </xsl:element>
      <xsl:element name="TermExpression">
        <xsl:attribute name="type" select="'Else'" />
        <xsl:attribute name="openCount" select="xs:integer(0)" />
        <xsl:text>[^0-9a-zA-Z_]*?else[^0-9a-zA-Z_]*?</xsl:text>
      </xsl:element>
      <xsl:element name="TermExpression">
        <xsl:attribute name="type" select="'Semi'" />
        <xsl:attribute name="openCount" select="xs:integer(0)" />
        <xsl:text>;\s*$</xsl:text>
      </xsl:element>
    </xsl:variable>
    <xsl:variable name="delimitedCode">
      <xsl:variable name="delimRegEx" select="concat('((', string-join($lineDelims/TermExpression, ')|('), '))')" />
      <xsl:for-each select="$processedCode/Code" >
        <xsl:analyze-string select="." regex="{$delimRegEx}">
          <xsl:matching-substring>
            <xsl:element name="CodeDelim">
              <xsl:variable name="term" select="for $i in 1 to count($lineDelims/TermExpression) return $lineDelims/TermExpression[$i][matches(regex-group(1), .)]" />
              <xsl:attribute name="DelimType" select="$term/@type" />
              <xsl:attribute name="OpenCount" select="$term/@openCount" />
              <xsl:value-of select="regex-group(1)" />
            </xsl:element>
          </xsl:matching-substring>
          <xsl:non-matching-substring>
            <xsl:element name="CodePart">
              <xsl:value-of select="normalize-space(.)" />
            </xsl:element>
          </xsl:non-matching-substring>
        </xsl:analyze-string>
      </xsl:for-each>
    </xsl:variable>
    <xsl:variable name="functionXML">
      <xsl:variable name="blockIDTable">
        <xsl:for-each select="$delimitedCode/CodeDelim">
          <xsl:if test=". eq 'CloseBrace'">
            <xsl:element name="BlockEntry">
              <xsl:element name="BlockID">
                <xsl:value-of select="count(preceding-sibling::CodeDelim[. eq 'OpenBrace'])" />
              </xsl:element>
              <xsl:element name="CloseNdx">
                <xsl:value-of select="position()" />
              </xsl:element>
            </xsl:element>
          </xsl:if>
        </xsl:for-each>
      </xsl:variable>
      <xsl:for-each select="$delimitedCode/CodeDelim">
        <xsl:variable name="ndx" select="position()" />
        <xsl:variable name="code" select="preceding-sibling::CodePart[1]" />
        <xsl:variable name="delim" select="." />
        <xsl:element name="Code">
          <xsl:choose>
            <xsl:when test="count(preceding-sibling::CodeDelim) eq 0">
              <xsl:attribute name="Depth" select="xs:integer(0)" />
            </xsl:when>
            <xsl:when test="matches(@DelimType, '(CloseBrace|BraceElse|BraceElseBrace)')">
              <xsl:attribute name="Depth" select="sum(preceding-sibling::CodeDelim/@OpenCount) - 1" />
            </xsl:when>
            <xsl:when test="matches(@DelimType, '(OpenBrace|ElseBrace)')">
              <xsl:attribute name="Depth" select="sum(preceding-sibling::CodeDelim/@OpenCount)" />
            </xsl:when>
            <xsl:when test="matches(@DelimType, '(Else|TerminatingParen)')">
              <xsl:attribute name="Depth" select="sum(preceding-sibling::CodeDelim/@OpenCount)" />
            </xsl:when>
            <xsl:otherwise>
              <xsl:attribute name="Depth" select="sum(preceding-sibling::CodeDelim/@OpenCount)" />
            </xsl:otherwise>
          </xsl:choose>
          <xsl:attribute name="Position" select="position()" />
          <xsl:choose>
            <xsl:when test="$delim/@DelimType eq 'Semi'">
              <xsl:attribute name="BlockID" select="'-1'" />
              <xsl:attribute name="CodeType" select="'Line'" />
              <xsl:value-of select="concat($code, ';')" />
            </xsl:when>
            <xsl:when test="$delim/@DelimType eq 'Return'">
              <xsl:attribute name="BlockID" select="'-1'" />
              <xsl:attribute name="CodeType" select="'Return'" />
              <xsl:if test="following-sibling::CodePart[1]/@DelimType ne 'code'">
                <xsl:value-of select="'_l._hr = false; return;'" />
              </xsl:if>
              <xsl:if test="following-sibling::CodePart[1]/@DelimType eq 'code'">
                <xsl:value-of select="concat('_l._hr = true; _l._rv = ', following-sibling::CodePart[1], '; return;')" />
              </xsl:if>
            </xsl:when>
            <xsl:when test="$delim/@DelimType eq 'Else'">
              <xsl:attribute name="BlockID" select="'-1'" />
              <xsl:attribute name="ParentType" select="'Else'" />
              <xsl:attribute name="CodeType" select="'Parent'" />
            </xsl:when>
            <xsl:when test="$delim/@DelimType eq 'BraceElse'">
              <xsl:attribute name="BlockID" select="'-1'" />
              <xsl:attribute name="CodeType" select="'Parent'" />
              <xsl:attribute name="ParentType" select="'BraceElse'" />
            </xsl:when>
            <xsl:when test="$delim/@DelimType eq 'ElseBrace'">
              <xsl:attribute name="BlockID" select="'-1'" />
              <xsl:attribute name="CodeType" select="'OpenBlock'" />
              <xsl:attribute name="BlockType" select="'Else'" />
            </xsl:when>
            <xsl:when test="$delim/@DelimType eq 'BraceElseBrace'">
              <xsl:attribute name="BlockID" select="'-1'" />
              <xsl:attribute name="CodeType" select="'NewBlock'" />
              <xsl:attribute name="BlockType" select="'Else'" />
            </xsl:when>
            <xsl:when test="$delim/@DelimType eq 'TerminatingParen'">
              <xsl:attribute name="BlockID" select="'-1'" />
              <xsl:attribute name="CodeType" select="'Parent'" />
              <xsl:choose>
                <xsl:when test="starts-with(lower-case($code), 'for')">
                  <xsl:attribute name="ParentType" select="'for'" />
                  <xsl:variable name="var" select="replace(substring-after($code, 'for'), '^\s*\((var\s+)?([_A-Za-z][0-9A-Za-z_]+).*?$', '$2')" />
                  <xsl:attribute name="Var" select="$var" />
                  <xsl:attribute name="StartValue" select="replace(concat(normalize-space(substring-after($code, '=')), ')'), '^(.+?)(.*)$', '$1')" />
                  <xsl:variable name="comparison" select="replace(normalize-space(substring-after($code, ';')), '^(.+?)(;.*)$', '$1')" />
                  <xsl:attribute name="Comparison" select="$comparison" />
                  <xsl:attribute name="VarChange" select="normalize-space(replace(normalize-space(substring-after($code, $comparison)), ';(.+?)$', '$1'))" />
                </xsl:when>
                <xsl:when test="starts-with(lower-case(normalize-space($code)), 'while')">
                  <xsl:attribute name="ParentType" select="'while'" />
                  <xsl:attribute name="Condition" select="replace(normalize-space(substring-after($code, '(')), '^(.+)$', '$1')" />
                </xsl:when>
                <xsl:when test="starts-with(lower-case(normalize-space($code)), 'if')">
                  <xsl:attribute name="ParentType" select="'if'" />
                  <xsl:attribute name="Condition" select="replace(normalize-space(substring-after($code, '(')), '^(.+)$', '$1')" />
                </xsl:when>
              </xsl:choose>
            </xsl:when>
            <xsl:when test="$delim/@DelimType eq 'OpenBrace'">
              <xsl:attribute name="BlockID" select="count(preceding-sibling::CodeDelim[. eq 'OpenBrace']) + 1" />
              <xsl:attribute name="CodeType" select="'OpenBlock'" />
              <xsl:choose>
                <xsl:when test="starts-with(lower-case($code), 'for')">
                  <xsl:attribute name="BlockType" select="'for'" />
                  <xsl:variable name="var" select="replace(substring-after($code, 'for'), '^\s*\((var\s+)?([_A-Za-z][0-9A-Za-z_]+).*?$', '$2')" />
                  <xsl:attribute name="Var" select="$var" />
                  <xsl:attribute name="StartValue" select="replace(concat(normalize-space(substring-after($code, '=')), ')'), '^(.+?)(;.*)$', '$1')" />
                  <xsl:variable name="comparison" select="replace(normalize-space(substring-after($code, ';')), '^(.+?)(;.*)$', '$1')" />
                  <xsl:attribute name="Comparison" select="$comparison" />
                  <xsl:attribute name="VarChange" select="normalize-space(replace(concat(normalize-space(substring-before(substring-after($code, $comparison), ')')), ')'), ';(.+?)\)$', '$1'))" />
                </xsl:when>
                <xsl:when test="starts-with(lower-case($code), 'while')">
                  <xsl:attribute name="BlockType" select="'while'" />
                  <xsl:attribute name="Condition" select="replace(normalize-space(substring($code, 2, string-length($code) - 2)), '^(.+)$', '$1')" />
                </xsl:when>
                <xsl:when test="starts-with(lower-case($code), 'do')">
                  <xsl:attribute name="BlockType" select="'do'" />
                </xsl:when>
                <xsl:when test="starts-with(lower-case(normalize-space($code)), 'if')">
                  <xsl:attribute name="BlockType" select="'if'" />
                  <xsl:attribute name="Condition" select="replace(normalize-space(substring-after($code, '(')), '^(.+)$', '$1')" />
                </xsl:when>
                <xsl:otherwise>
                  <xsl:attribute name="BlockType" select="'none'"  />
                </xsl:otherwise>
              </xsl:choose>
            </xsl:when>
            <xsl:when test="$delim/@DelimType eq 'CloseBrace'">
              <xsl:variable name="pos" select="xs:integer(position())" />
              <xsl:attribute name="BlockID" select="$blockIDTable[xs:integer(CloseNdx) eq $pos]/BlockID" />
              <xsl:attribute name="CodeType" select="'CloseBlock'" />
              <xsl:if test="following-sibling::*[1]/name() eq 'CodePart'">
                <xsl:variable name="followingCode" select="normalize-space(following-sibling::CodePart[1])" />
                <xsl:if test="starts-with($followingCode, 'while')">
                  <xsl:attribute name="BlockTermType" select="'DoWhile'" />
                  <xsl:variable name="whileClause" select="normalize-space(substring-after($followingCode, 'while'))" />
                  <xsl:attribute name="Condition" select="substring($whileClause, 2, string-length($followingCode) - 2)" />
                </xsl:if>
                <xsl:if test="not(starts-with($followingCode, 'while'))">
                  <xsl:attribute name="BlockTermType" select="'Term'" />
                </xsl:if>
              </xsl:if>
              <xsl:if test="following-sibling::*[1]/name() ne 'CodePart'">
                <xsl:attribute name="BlockTermType" select="'Term'" />
              </xsl:if>
              <xsl:if test="count(following-sibling::*) eq 0">
                <xsl:attribute name="BlockTermType" select="'Term'" />
              </xsl:if>
            </xsl:when>
          </xsl:choose>
        </xsl:element>
      </xsl:for-each>
    </xsl:variable>
    <xsl:variable name="maxDepth" select="max(xs:integer($functionXML/Code/@Depth))" />
    <xsl:variable name="sequences">
      <xsl:for-each select="$functionXML/Code[(position() eq 1) or (@Depth ne preceding-sibling::Code[1]/@Depth)]">
        <xsl:variable name="segDepth" select="@Depth" />
        <xsl:variable name="codeNode" select="." />
        <xsl:variable name="length">
          <xsl:if test="position() eq last()">
            <xsl:value-of select="count(following-sibling::Code) + 1" />
          </xsl:if>
          <xsl:if test="position() ne last()">
            <xsl:value-of select="count(following-sibling::Code[@Depth eq $segDepth][every $p in preceding-sibling::Code intersect $codeNode/following-sibling::Code satisfies $p/@Depth eq $segDepth]) + 1" />
          </xsl:if>
        </xsl:variable>
        <xsl:element name="CodeSequence">
          <xsl:variable name="startPos" select="xs:integer(@Position)" />
          <xsl:attribute name="Depth" select="$segDepth" />
          <xsl:attribute name="numFollowing" select="count(following-sibling::Code)" />
          <xsl:attribute name="Length" select="$length" />
          <xsl:attribute name="SequenceNum" select="position()" />
          <xsl:copy-of select="(., following-sibling::Code[position() lt xs:integer($length)])" />
        </xsl:element>
      </xsl:for-each>
    </xsl:variable>
    <xsl:variable name="formattedSequences">
      <xsl:for-each select="$sequences/CodeSequence">
        <xsl:variable name="thisSequence" select="." />
        <xsl:element name="CodeSequence">
          <xsl:attribute name="Depth" select="@Depth" />
          <xsl:attribute name="Position" select="position()" />
          <xsl:attribute name="OpenRole" select="Code[1]/@CodeType" />
          <xsl:attribute name="CloseRole" select="Code[last()]/@CodeType" />
          <xsl:variable name="startNdx" >
            <xsl:if test="$thisSequence/Code[1]/@CodeType ne 'Line'">
              <xsl:value-of select="'2'" />
            </xsl:if>
            <xsl:if test="$thisSequence/Code[1]/@CodeType eq 'Line'">
              <xsl:value-of select="'1'" />
            </xsl:if>
          </xsl:variable>
          <xsl:variable name="endNdx">
            <xsl:if test="Code[last()]/@CodeType ne 'Line'">
              <xsl:value-of select="last() - 1" />
            </xsl:if>
            <xsl:if test="Code[last()]/@CodeType eq 'Line'">
              <xsl:value-of select="last()" />
            </xsl:if>
          </xsl:variable>
          <xsl:for-each select="$thisSequence/Code">
            <xsl:if test="@CodeType eq 'Line'">
              <xsl:element name="Line">
                <xsl:value-of select="." />
              </xsl:element>
            </xsl:if>
            <xsl:if test="@CodeType ne 'Line'">
              <xsl:call-template name="OutputNonLine">
                <xsl:with-param name="elem" select="." />
              </xsl:call-template>
            </xsl:if>
          </xsl:for-each>
        </xsl:element>
      </xsl:for-each>
    </xsl:variable>
    <xsl:variable name="ParamTable">
      <xsl:for-each select="$params/Param">
        <xsl:element name="ParamEntry">
          <xsl:element name="OrigParam">
            <xsl:value-of select="." />
          </xsl:element>
          <xsl:element name="NewParam">
            <xsl:value-of select="concat('_p', position())" />
          </xsl:element>
        </xsl:element>
      </xsl:for-each>
    </xsl:variable>
    <xsl:variable name="depths">
      <xsl:for-each select="$formattedSequences/CodeSequence/@Depth">
        <xsl:element name="Depth">
          <xsl:value-of select="xs:integer(.)" />
        </xsl:element>
      </xsl:for-each>
    </xsl:variable>
    <xsl:for-each select="0 to xs:integer(max($depths/Depth))">
      <xsl:variable name="depth" select="." />
      <xsl:if test="$depth eq 0">
        <xsl:call-template name="ConstructSubFunction">
          <xsl:with-param name="functionSegments" select="$formattedSequences" />
          <xsl:with-param name="params" select="$ParamTable/ParamEntry/NewParam" />
          <xsl:with-param name="functName" select="$functionName" />
          <xsl:with-param name="segNum" select="0" />
        </xsl:call-template>
      </xsl:if>
      <xsl:if test="$depth gt 0">
        <xsl:variable name="thisDepthSequences">
          <xsl:for-each select="$formattedSequences/CodeSequence">
            <xsl:variable name="codeSeq" select="." />
            <xsl:variable name="codePos" select="position()" />
            <xsl:element name="newSeqNdx">
              <xsl:value-of select="if (xs:integer($codeSeq/@Depth) ge $depth) then xs:integer($codePos) else -1" />
            </xsl:element>
          </xsl:for-each>
        </xsl:variable>
        <xsl:for-each select="$thisDepthSequences/newSeqNdx[(xs:integer(.) ne -1) and ((xs:integer(.) eq 0) or (xs:integer(preceding-sibling::newSeqNdx[1]) eq -1))]" >
          <xsl:variable name="codePos" select="xs:integer(.)" />
          <xsl:variable name="subSeqs">
            <xsl:for-each select="distinct-values($thisDepthSequences/newSeqNdx[position() ge $codePos][xs:integer(.) ne -1][(every $i in preceding-sibling::newSeqNdx[xs:integer(.) ge $codePos] satisfies xs:integer(.) ne -1)])" >
              <xsl:variable name="seqPos" select="xs:integer(.)" />
              <xsl:copy-of select="$formattedSequences/CodeSequence[$seqPos]" />
            </xsl:for-each>
          </xsl:variable>
          <xsl:call-template name="ConstructSubFunction">
            <xsl:with-param name="functionSegments" select="$subSeqs" />
            <xsl:with-param name="params" select="if ($depth eq 0) then $ParamTable/ParamEntry/NewParam else ()" />
            <xsl:with-param name="functName" select="$functionName" />
            <xsl:with-param name="segNum" select="position()" />
          </xsl:call-template>
        </xsl:for-each>
      </xsl:if>
    </xsl:for-each>
  </xsl:template>

  <xsl:template name="ConstructSubFunction">
    <xsl:param name="functionSegments" />
    <xsl:param name="params" />
    <xsl:param name="numLocals" />
    <xsl:param name="functName" />
    <xsl:param name="segNum" />
    <xsl:variable name="parentDepth" select="min(for $i in 1 to count($functionSegments) return $functionSegments/CodeSequence[$i]/@Depth)" />
    <xsl:variable name="cSegs" select="$functionSegments/CodeSequence" />
    <xsl:variable name="segItrVals" select="for $i in 1 to count($cSegs[xs:integer(@Depth) eq $parentDepth]) return index-of($cSegs/@Position, $cSegs[xs:integer(@Depth) eq $parentDepth][$i]/@Position)" />
    <xsl:element name="SubFunction">
      <xsl:attribute name="Params" select="$params" />
      <xsl:attribute name="FunctionName">
        <xsl:if test="$parentDepth eq 0">
          <xsl:value-of select="$functName" />
        </xsl:if>
        <xsl:if test="$parentDepth gt 0">
          <xsl:value-of select="concat($functName, '.s', $parentDepth, '_', $segNum)" />
        </xsl:if>
      </xsl:attribute>
      <xsl:for-each select="1 to count($segItrVals)">
        <xsl:variable name="segPosNdx" select="." />
        <xsl:variable name="segPos" select="$segItrVals[$segPosNdx]" />
        <xsl:if test="$segPosNdx eq 1">
          <xsl:if test="$parentDepth eq 0">
            <xsl:element name="Line">
              <xsl:value-of select="'var _l = new Object();'"/>
            </xsl:element>
            <xsl:element name="Line">
              <xsl:value-of select="'_l._hr = false;'" />
            </xsl:element>
            <xsl:element name="Line">
              <xsl:value-of select="'_l._rv = null;'" />
            </xsl:element>
          </xsl:if>
        </xsl:if>
        <xsl:copy-of select="$functionSegments/CodeSequence[$segPos]/Line" />
        <xsl:if test="$segPos ne ($segItrVals[$segPosNdx + 1] - 1)">
          <xsl:variable name="subDepth" select="$parentDepth + 1" />
          <xsl:variable name="subFunctName" select="concat($functName, '.s', $subDepth, '_', $segPos - $segPosNdx + 1)" />
          <xsl:element name="Line">
            <xsl:value-of select="concat($subFunctName, '.eval(_l);')" />
          </xsl:element>
          <xsl:element name="Line">
            <xsl:value-of select="'if (_l._hr == true) return _l._rv;'"/>
          </xsl:element>
        </xsl:if>
      </xsl:for-each>
    </xsl:element>
  </xsl:template>

  <xsl:template name="OutputNonLine">
    <xsl:param name="elem" />
    <xsl:variable name="elemType">
      <xsl:choose>
        <xsl:when test="$elem/@CodeType eq 'Parent'">
          <xsl:value-of select="$elem/@ParentType" />
        </xsl:when>
        <xsl:when test="$elem/@CodeType eq 'OpenBlock'">
          <xsl:value-of select="$elem/@BlockType" />
        </xsl:when>
        <xsl:when test="$elem/@CodeType eq 'CloseBlock'">
          <xsl:value-of select="$elem/@BlockTermType" />
        </xsl:when>
      </xsl:choose>
    </xsl:variable>
    <xsl:element name="Line">
      <xsl:choose>
        <xsl:when test="$elemType eq 'Else'">
          <xsl:value-of select="'else'" />
        </xsl:when>
        <xsl:when test="$elemType eq 'for'">
          <xsl:value-of select="concat('for (', (if (@VarDeclared) then 'var' else ''), @Var, ' = ', @StartValue, '; ', @Comparison, '; ', @VarChange, ')', (if ($elem/@CodeType eq 'OpenBlock') then ' {' else ''))" />
        </xsl:when>
        <xsl:when test="$elemType eq 'while'">
          <xsl:value-of select="concat('while (', @Condition, ')', (if (@CodeType eq 'OpenBlock') then '{' else ''))" />
        </xsl:when>
        <xsl:when test="$elemType eq 'do'">
          <xsl:value-of select="'do {'" />
        </xsl:when>
        <xsl:when test="$elemType eq 'DoWhile'">
          <xsl:value-of select="concat('while (', @Condition, ');')" />
        </xsl:when>
        <xsl:when test="$elemType eq 'Term'">
          <xsl:value-of select="'}'" />
        </xsl:when>
        <xsl:when test="$elemType eq 'if'">
          <xsl:value-of select="concat('if (', @Condition, ')', if ($elem/@CodeType eq 'OpenBlock') then '{' else '')" />
        </xsl:when>
        <xsl:when test="$elemType eq 'none'">
          <xsl:if test="$elem/@CodeType eq 'OpenBlock'">
            <xsl:value-of select="'{'" />
          </xsl:if>
        </xsl:when>
      </xsl:choose>
    </xsl:element>
  </xsl:template>

  <xsl:template name="ConstructClass">
    <xsl:param name="class"/>
    <xsl:param name="nameBase" />
    <xsl:element name="Class">
      <xsl:attribute name="ClassName" select="$class/@ClassName" />
      <xsl:element name="Constructor">
        <xsl:copy-of select="$class/Constructor/Params" />
        <xsl:variable name="mungedCode">
          <xsl:call-template name="mungeFunction">
            <xsl:with-param name="params" select="$class/Constructor/Params/Param" />
            <xsl:with-param name="varDeclLine" select="$class/Constructor/VarDecls" />
            <xsl:with-param name="codeLines" select="$class/Constuctor/Code" />
          </xsl:call-template>
        </xsl:variable>
        <xsl:call-template name="ConstructFunction">
          <xsl:with-param name="functionName" select="$nameBase" />
          <xsl:with-param name="processedCode" select="$mungedCode" />
          <xsl:with-param name="params" select="$class/Constructor/Params" />
        </xsl:call-template>
      </xsl:element>
      <xsl:element name="PrototypeChain">
        <xsl:for-each select="$class/PrototypeChain/Function">
          <xsl:element name="MemberFunction">
            <xsl:attribute name="FunctionName" select="@FunctionName" />
            <xsl:copy-of select="Params" />
            <xsl:variable name="mungedCode">
              <xsl:call-template name="mungeFunction">
                <xsl:with-param name="params" select="$class/Constructor/Params/Param" />
                <xsl:with-param name="varDeclLine" select="VarDecls" />
                <xsl:with-param name="codeLines" select="Code" />
              </xsl:call-template>
            </xsl:variable>
            <xsl:call-template name="ConstructFunction">
              <xsl:with-param name="functionName" select="concat($nameBase, '.F', position())" />
              <xsl:with-param name="processedCode" select="$mungedCode" />
              <xsl:with-param name="params" select="$function/Params/Param" />
            </xsl:call-template>
          </xsl:element>
        </xsl:for-each>
      </xsl:element>
    </xsl:element>
  </xsl:template>

  <xsl:template name="WriteClassCode">
    <xsl:param name="class" />
    <xsl:variable name="memberVariables">
      <xsl:for-each select="$class//Line">
        <xsl:analyze-string select="." regex="^this\.([A-Za-z_][A-Za-z0-9_]*)([^A-Za-z0-9\.\|_])" >
          <xsl:matching-substring>
            <xsl:if test="every $mf in $class/PrototypeChain/MemberFunction satisfies $mf/@FunctionName ne .">
              <xsl:element name="MemberVariable">
                <xsl:value-of select="concat('this.', regex-group(1))" />
              </xsl:element>
            </xsl:if>
          </xsl:matching-substring>
        </xsl:analyze-string>
      </xsl:for-each>
    </xsl:variable>
    <xsl:variable name="mvTable">
      <xsl:for-each select="distinct-values($memberVariables/MemberVariable)">
        <xsl:element name="Entry">
          <xsl:element name="OrigDecl">
            <xsl:value-of select="." />
          </xsl:element>
          <xsl:element name="NewDecl">
            <xsl:value-of select="concat('this._mv', position())"/>
          </xsl:element>
        </xsl:element>
      </xsl:for-each>
    </xsl:variable>
    <xsl:variable name="mfRegEx" select="concat('(^|[^A-Za-z0-9\.\|_])(', string-join($memberVariables/MemberVariable, '|'), ')([^A-Za-z0-9_])')" />
    <xsl:element name="Class">
      <xsl:attribute name="ClassName" select="$class/@ClassName" />
      <xsl:element name="ClassFunctions">
        <xsl:for-each select="$class//SubFunction">
          <xsl:element name="Function">
            <xsl:attribute name="Name" select="@FunctionName" />
            <xsl:for-each select="Line">
              <xsl:analyze-string select="." regex="{$mfRegEx}">
                <xsl:matching-substring>
                  <xsl:value-of select="concat(regex-group(1), $mvTable/Entry[OrigDecl eq regex-group(2)]/NewDecl, regex-group(3))" />
                </xsl:matching-substring>
                <xsl:non-matching-substring>
                  <xsl:value-of select="." />
                </xsl:non-matching-substring>
              </xsl:analyze-string>
              <xsl:if test="position() ne last()">
                <xsl:value-of select="' '"/>
              </xsl:if>
            </xsl:for-each>
          </xsl:element>
        </xsl:for-each>
      </xsl:element>
      <xsl:variable name="classCode">
        <xsl:element name="ConstructorBody">
          <xsl:variable name="conParams" select="string-join($class/Constructor/Params/Param, ', ')" />
          <xsl:value-of select="concat('function ', $class/@ClassName, '(', $conParams, ') { ', $class/Constructor/SubFunction[1]/@FunctionName, '.eval(new Array(', $conParams, ')); }')" />
        </xsl:element>
        <xsl:element name="PrototypeDefStart">
          <xsl:value-of select="concat($class/@ClassName, '.prototype = { constructor: ', $class/@ClassName, ', ')" />
        </xsl:element>
        <xsl:element name="MemberFunctionDefs">
          <xsl:for-each select="$class/PrototypeChain/MemberFunction">
            <xsl:element name="Def">
              <xsl:variable name="funParams" select="string-join(Params/Param, ', ')" />
              <xsl:value-of select="concat(@FunctionName, ' : function(', $funParams, ') {', SubFunction[1]/@FunctionName, '.eval(new Array(', $funParams, ')); }')"/>
            </xsl:element>
          </xsl:for-each>
        </xsl:element>
        <xsl:element name="PrototypeDefEnd">
          <xsl:value-of select="'};'"/>
        </xsl:element>
      </xsl:variable>
      <xsl:element name="CodeLine">
        <xsl:value-of select="concat($classCode/ConstructorBody, $classCode/PrototypeDefStart, string-join($classCode/MemberFunctionDefs/Def, ', '), $classCode/PrototypeDefEnd)" />
      </xsl:element>
    </xsl:element>
  </xsl:template>

  <xsl:template name="WriteFunctionCode">
    <xsl:param name="functionName" />
    <xsl:param name="params" />
    <xsl:param name="subFuncts" />
    <xsl:element name="GlobalFunction">
      <xsl:attribute name="FunctionName" select="$functionName" />
      <xsl:element name="CompositeFunctions">
        <xsl:for-each select="$subFuncts/SubFunction">
          <xsl:element name="Function">
            <xsl:attribute name="Name" select="@FunctionName" />
            <xsl:value-of select="string-join(Line, ' ')" />
          </xsl:element>
        </xsl:for-each>
      </xsl:element>
      <xsl:variable name="paramList" select="string-join($params/Param, ', ')" />
      <xsl:element name="CodeLine">
        <xsl:value-of select="concat('function ', $functionName, '(', $paramList, ') { ', $subFuncts/SubFunction[1]/@FunctionName, '.eval(new Array(', $paramList, ')); }')"/>
      </xsl:element>
    </xsl:element>
  </xsl:template>
</xsl:stylesheet>
