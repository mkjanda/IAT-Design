<?xml version="1.0" encoding="UTF-8"?>
<xsl:stylesheet xmlns:xs="http://www.w3.org/2001/XMLSchema"
                xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
                version="2.0"
                exclude-result-prefixes="xs">

  <xsl:output method="xml" encoding="utf-8" indent="yes" cdata-section-elements="Function Declaration FunctionConstructor"/>

  <xsl:template match="//GlobalCode">
    <xsl:call-template name="ConstructFunction">
      <xsl:with-param name="functionName" select="'Global'" />
      <xsl:with-param name="functionCode" select="string-join(Code, '&#x0A;')" />
      <xsl:with-param name="params" select="''" />
    </xsl:call-template>
  </xsl:template>
  
  <xsl:template match="//Class">
    <xsl:variable name="classNdx" select="ClassNdx" />
    <xsl:variable name="class">
      <xsl:element name="Constructor">
        <xsl:copy-of select="Constructor/Params" />
        <xsl:call-template name="ConstructFunction">
          <xsl:with-param name="functionName" select="concat('C', $classNdx)" />
          <xsl:with-param name="functionCode" select="string-join(Constructor/FunctionBody/Code, '&#x0A;')" />
          <xsl:with-param name="params" select="Constructor/Params" />
        </xsl:call-template>
      </xsl:element>
      <xsl:element name="PrototypeChain">
        <xsl:for-each select="PrototypeChain/Function">
          <xsl:element name="Function">
            <xsl:attribute name="Name" select="@FunctionName" />
            <xsl:copy-of select="Params"/>
            <xsl:call-template name="ConstructFunction">
              <xsl:with-param name="functionName" select="concat('C', $classNdx, '.F', position())" />
              <xsl:with-param name="functionCode" select="string-join(FunctionBody/Code, '&#x0A;')" />
              <xsl:with-param name="params" select="Params" />
            </xsl:call-template>
          </xsl:element>
        </xsl:for-each>
      </xsl:element>
    </xsl:variable>
    <xsl:element name="ProcessedCode">
      <xsl:attribute name="Name" select="concat('C', $classNdx)" />
      <xsl:variable name="classDecl">
        <xsl:variable name="conParams">
          <xsl:value-of select="string-join(Constructor/Params/Param, ', ')" />
        </xsl:variable>
        <xsl:element name="Part">
          <xsl:value-of select="concat('function ', @ClassName, '(', $conParams, ') { C', $classNdx, '.eval(new Array(', $conParams, ')); ')"/>
        </xsl:element>
        <xsl:element name="Part">
          <xsl:value-of select="concat(@ClassName, '.prototype = { constructor : ', @ClassName)" />
        </xsl:element>
        <xsl:element name="protoParts">
          <xsl:for-each select="PrototypeChain/Function">
            <xsl:variable name="funParams">
              <xsl:value-of select="string-join(Params/Param, ', ')"/>
            </xsl:variable>
            <xsl:element name="Part">
              <xsl:value-of select="concat(', ', @FunctionName, ' : function(', $funParams, ') { C', $classNdx, '.F', position(), '.eval(new Array(', $funParams, ')); }')"/>
            </xsl:element>
          </xsl:for-each>
        </xsl:element>
        <xsl:element name="Part">
          <xsl:value-of select="'};'"/>
        </xsl:element>
      </xsl:variable>
      <xsl:element name="Declaration">
        <xsl:value-of select="string-join($classDecl//Part, '')"/>
      </xsl:element>
      <xsl:element name="FunctionConstructor">
        <xsl:variable name="functInstantiations">
          <xsl:for-each select="$class//SubFunction">
            <xsl:element name="FunctName">
              <xsl:value-of select="@FunctionName" />
            </xsl:element>
          </xsl:for-each>
        </xsl:variable>
        <xsl:value-of select="concat('var ', string-join(for $n in $functInstantiations/FunctName return concat($n, ' = new SubFunct(&quot;', $n, '&quot;);'), ' '))" />
      </xsl:element>
      <xsl:variable name="subFuncts">
        <xsl:for-each select="$class//SubFunction">
          <xsl:element name="Function">
            <xsl:attribute name="Name" select="@FunctionName" />
            <xsl:copy-of select="Line" />
          </xsl:element>
        </xsl:for-each>
      </xsl:variable>
      <xsl:variable name="memberVariables">
        <xsl:for-each select="$subFuncts//Line">
          <xsl:analyze-string select="." regex="^this\.([A-Za-z_][A-Za-z0-9_]*)" >
            <xsl:matching-substring>
              <xsl:if test="every $mf in $class/PrototypeChain/Function satisfies $mf/@Name ne regex-group(1)">
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
      <xsl:variable name="mvRegEx" select="concat('(^|[^A-Za-z0-9_\.])(', string-join($mvTable/Entry/OrigDecl, '|'), ')([^A-Za-z0-9_])')" />
      <xsl:element name="Functions">
      <xsl:for-each select="$subFuncts/Function">
        <xsl:element name="Function">
          <xsl:attribute name="Name" select="@Name" />
          <xsl:analyze-string select="string-join(Line, ' ')" regex="{$mvRegEx}">
            <xsl:matching-substring>
              <xsl:value-of select="concat(regex-group(1), $mvTable/Entry[OrigDecl eq regex-group(2)]/NewDecl, regex-group(3))" />
            </xsl:matching-substring>
            <xsl:non-matching-substring>
              <xsl:value-of select="." />
            </xsl:non-matching-substring>
          </xsl:analyze-string>
        </xsl:element>
      </xsl:for-each>
      </xsl:element>
    </xsl:element>
  </xsl:template>

  <xsl:template match="//Function">
    <xsl:variable name="functNdx" select="FunctionNdx" />
    <xsl:variable name="function">
      <xsl:copy-of select="Params"/>
      <xsl:call-template name="ConstructFunction">
        <xsl:with-param name="functionName" select="concat('F', $functNdx)" />
        <xsl:with-param name="functionCode" select="string-join(FunctionBody/Code, '&#x0A;')" />
        <xsl:with-param name="params" select="Params" />
      </xsl:call-template>
    </xsl:variable>
    <xsl:element name="ProcessedCode">
      <xsl:attribute name="Name" select="concat('F', $functNdx)" />
      <xsl:element name="Declaration">
        <xsl:variable name="params" select="string-join($function/Params/Param, ', ')" />
        <xsl:value-of select="concat('function ', @FunctionName, '(', $params, ') { F', $functNdx, '.eval(new Array(', $params, ')); }')"/>
      </xsl:element>
      <xsl:element name="FunctionConstructor">
        <xsl:variable name="functInstantiations">
          <xsl:for-each select="$function//SubFunction">
            <xsl:element name="FunctName">
              <xsl:value-of select="@FunctionName" />
            </xsl:element>
          </xsl:for-each>
        </xsl:variable>
        <xsl:value-of select="concat('var ', string-join(for $n in $functInstantiations/FunctName return concat($n, ' = new SubFunct(&quot;', $n, '&quot;);'), ' '))" />
      </xsl:element>
      <xsl:element name="Functions">
      <xsl:for-each select="$function//SubFunction">
        <xsl:element name="Function">
          <xsl:attribute name="Name" select="@FunctionName" />
          <xsl:value-of select="string-join(Line, ' ')" />
        </xsl:element>
      </xsl:for-each>
      </xsl:element>
    </xsl:element>
  </xsl:template>

  <xsl:template name="processCode">
    <xsl:param name="code"/>
    <xsl:param name="type"/>
    <xsl:param name="delim"/>
    <xsl:variable name="codeList">
      <xsl:analyze-string select="$code" regex="(([\)\{{\}};])|else)\s*?&#xA;">
        <xsl:non-matching-substring>
          <xsl:variable name="line" select="normalize-space(.)"/>
          <xsl:choose>
            <xsl:when test="matches($line, '^var\s+?([A-Za-z_][A-Za-z0-9_]*)(\s*=((\s+|[^;=/,&#34;\(]+?|&#34;[^&#34;\n\r]*?&#34;|\(([^;=,&#34;]*?,?(&#34;[^\n\r&#34;]*?&#34;)?)+\)|/[^/\n]+?/)+?)?)*')">
              <xsl:analyze-string select="replace($line, '(var\s+?)(.+)', '$2')"
                                  regex="([A-Za-z_][A-Za-z0-9_]*)(\s*(=((\s+|[^;=/,&#34;\(]+?|&#34;[^&#34;\n\r]*?&#34;|\(([^;=,&#34;]*?,?(&#34;[^\n\r&#34;]*?&#34;)?)+\)|/[^/\n]+?/)*)+?)?)">
                <xsl:matching-substring>
                  <xsl:element name="code">
                    <xsl:attribute name="type" select="'varName'"/>
                    <xsl:value-of select="regex-group(1)" />
                  </xsl:element>
                  <xsl:if test="string-length(regex-group(2)) gt 0">
                    <xsl:element name="code">
                      <xsl:attribute name="type" select="'varAssign'"/>
                      <xsl:value-of select="regex-group(2)" />
                    </xsl:element>
                  </xsl:if>
                  <xsl:element name="code">
                    <xsl:attribute name="type" select="'subLineDelim'"/>
                    <xsl:value-of select="';'"/>
                  </xsl:element>
                </xsl:matching-substring>
              </xsl:analyze-string>
            </xsl:when>
            <xsl:when test="matches($line, '([^A-Za-z0-9_])(var\s+)([A-Za-z0-9][A-Za-z0-9_]+)')">
              <xsl:analyze-string select="$line" regex="([^A-Za-z0-9_])(var\s+)([A-Za-z0-9][A-Za-z0-9_]+)">
                <xsl:matching-substring>
                  <xsl:element name="code">
                    <xsl:attribute name="type" select="'code'"/>
                    <xsl:value-of select="regex-group(1)" />
                  </xsl:element>
                  <xsl:element name="code">
                    <xsl:attribute name="type" select="'varName'"/>
                    <xsl:value-of select="regex-group(3)" />
                  </xsl:element>
                </xsl:matching-substring>
                <xsl:non-matching-substring>
                  <xsl:element name="code">
                    <xsl:attribute name="type" select="'code'"/>
                    <xsl:value-of select="." />
                  </xsl:element>
                </xsl:non-matching-substring>
              </xsl:analyze-string>
            </xsl:when>
            <xsl:otherwise>
              <xsl:element name="code">
                <xsl:attribute name="type" select="'code'"/>
                <xsl:value-of select="$line" />
              </xsl:element>
            </xsl:otherwise>
          </xsl:choose>
        </xsl:non-matching-substring>
        <xsl:matching-substring>
          <xsl:element name="code">
            <xsl:attribute name="type" select="'subLineDelim'"/>
            <xsl:value-of select="regex-group(1)" />
          </xsl:element>
        </xsl:matching-substring>
      </xsl:analyze-string>
    </xsl:variable>
    <xsl:if test="(($type eq 'vars') or ($type eq 'both')) and (count($codeList/code[@type eq 'varName']) gt 0)">
      <xsl:value-of select="'var '"/>
      <xsl:for-each select="$codeList/code[((@type eq 'varName') and (every $var in preceding-sibling::code[@type eq 'varName'] satisfies normalize-space(.) ne normalize-space($var))) or (@type eq 'varAssign')]">
        <xsl:variable name="varName" select="."/>
        <xsl:if test="@type eq 'varName'">
          <xsl:choose>
            <xsl:when test="(position() eq last()) and (every $var in preceding-sibling::code[@type eq 'varName'] satisfies $var ne $varName)">
              <xsl:value-of select="." />
            </xsl:when>
            <xsl:when test="(count(following-sibling::code[@type eq 'varName']) gt 0) and (some $followingVar in following-sibling::code[@type eq 'varName'] satisfies ($followingVar ne $varName) and (every $precedingVar in preceding-sibling::code[@type eq 'varName'] satisfies $precedingVar ne $followingVar)) and (following-sibling::code[1]/@type ne 'varAssign')">
              <xsl:value-of select="concat(., ', ')" />
            </xsl:when>
            <xsl:when test="following-sibling::code[1]/@type eq 'varAssign'">
              <xsl:variable name="assign" select="following-sibling::code[1]"/>
              <xsl:choose>
                <xsl:when test="some $var in preceding-sibling::code[@type eq 'varName'] satisfies (matches($assign, concat('[^A-Za-z0-9_]', normalize-space($var), '[^A-Za-z0-9_]?')) or matches($assign, concat('^', normalize-space($var), '[^A-Za-z0-9_]?')))">
                  <xsl:if test="every $elem in following-sibling::code[(@type eq 'varName') or (@type eq 'varAssign')] satisfies $elem/@type eq 'varAssign'">
                    <xsl:value-of select="." />
                  </xsl:if>
                  <xsl:if test="some $elem in following-sibling::code[(@type eq 'varName') or (@type eq 'varAssign')] satisfies $elem/@type ne 'varAssign'">
                    <xsl:value-of select="concat(., ', ')"/>
                  </xsl:if>
                </xsl:when>
                <xsl:when test="(every $var in preceding-sibling::code[@type eq 'varName'] satisfies $var ne $varName) and (every $var in following-sibling::code[@type eq 'varName'] satisfies $var ne $varName)">
                  <xsl:if test="position() + 1 eq last()">
                    <xsl:value-of select="concat(., following-sibling::code[1])"/>
                  </xsl:if>
                  <xsl:if test="position() + 1 ne last()">
                    <xsl:value-of select="concat(., following-sibling::code[1], ', ')"/>
                  </xsl:if>
                </xsl:when>
                <xsl:otherwise>
                  <xsl:choose>
                    <xsl:when test="(position() + 1 eq last()) or (every $followingVar in following-sibling::code[@type eq 'varName'] satisfies ($followingVar eq $varName) or (some $precedingVar in preceding-sibling::code[@type eq 'varName'] satisfies $followingVar eq $precedingVar))">
                      <xsl:value-of select="." />
                    </xsl:when>
                    <xsl:when test="position() + 1 ne last()">
                      <xsl:value-of select="concat(., ', ')"/>
                    </xsl:when>
                  </xsl:choose>
                </xsl:otherwise>
              </xsl:choose>
            </xsl:when>
          </xsl:choose>
        </xsl:if>
      </xsl:for-each>
      <xsl:value-of select="concat(';', $delim)"/>
    </xsl:if>
    <xsl:if test="($type eq 'code') or ($type eq 'both')">
      <xsl:for-each select="$codeList/code">
        <xsl:choose>
          <xsl:when test="@type eq 'code'">
            <xsl:value-of select="." />
          </xsl:when>
          <xsl:when test="(@type eq 'varAssign') and (preceding-sibling::code[1]/@type eq 'varName')">
            <xsl:variable name="assign" select="normalize-space(.)"/>
            <xsl:variable name="thisVarName" select="preceding-sibling::code[@type eq 'varName'][1]"/>
            <xsl:choose>
              <xsl:when test="some $var in preceding-sibling::code[@type eq 'varName'][position() gt 1] satisfies matches($assign, concat('[^A-Za-z0-9_]?', normalize-space($var), '[^A-Za-z0-9_]?'))">
                <xsl:value-of select="concat(preceding-sibling::code[1], .)" />
              </xsl:when>
              <xsl:when test="(some $var in preceding-sibling::code[@type eq 'varName'][position() gt 1] satisfies $var eq $thisVarName) or (some $var in following-sibling::code[@type eq 'varName'] satisfies $var eq $thisVarName)">
                <xsl:value-of select="concat(preceding-sibling::code[1], .)" />
              </xsl:when>
            </xsl:choose>
          </xsl:when>
          <xsl:when test="(@type eq 'varName') and (position() gt 1)">
            <xsl:if test="preceding-sibling::code[1]/@type eq 'code'">
              <xsl:value-of select="." />
            </xsl:if>
          </xsl:when>
          <xsl:when test="@type eq 'subLineDelim'">
            <xsl:if test="matches(., '^[^;]')">
              <xsl:value-of select="concat(., $delim)" />
            </xsl:if>
            <xsl:if test="matches(., '^;')">
              <xsl:choose>
                <xsl:when test="preceding-sibling::code[1]/@type eq 'varAssign'">
                  <xsl:if test="preceding-sibling::code[2]/@type eq 'varName'">
                    <xsl:variable name="assign"
                                  select="normalize-space(preceding-sibling::code[@type eq 'varAssign'][1])"/>
                    <xsl:variable name="thisVarName" select="preceding-sibling::code[@type eq 'varName'][1]"/>
                    <xsl:choose>
                      <xsl:when test="some $var in preceding-sibling::code[@type eq 'varName'][position() gt 1] satisfies matches($assign, concat('[^A-Za-z0-9_]?', normalize-space($var), '[^A-Za-z0-9_]?'))">
                        <xsl:value-of select="concat(., $delim)" />
                      </xsl:when>
                      <xsl:when test="(some $var in preceding-sibling::code[@type eq 'varName'][position() gt 1] satisfies $var eq $thisVarName) or (some $var in following-sibling::code[@type eq 'varName'] satisfies $var eq $thisVarName)">
                        <xsl:value-of select="concat(., $delim)" />
                      </xsl:when>
                    </xsl:choose>
                  </xsl:if>
                </xsl:when>
                <xsl:when test="preceding-sibling::code[1]/@type eq 'varName'">
                  <xsl:variable name="varName" select="preceding-sibling::code[1]"/>
                  <xsl:if test="position() gt 2">
                    <xsl:if test="every $elem in preceding-sibling::code[@type eq 'varName'] satisfies $elem ne $varName">
                      <xsl:value-of select="concat(., $delim)" />
                    </xsl:if>
                  </xsl:if>
                </xsl:when>
                <xsl:when test="preceding-sibling::code[1]/@type eq 'code'">
                  <xsl:value-of select="concat(., $delim)" />
                </xsl:when>
              </xsl:choose>
            </xsl:if>
          </xsl:when>
        </xsl:choose>
      </xsl:for-each>
    </xsl:if>
  </xsl:template>

  <xsl:template name="mungeFunction" >
    <xsl:param name="functionCode" />
    <xsl:param name="params" />
    <xsl:variable name="varDeclLine">
      <xsl:call-template name="processCode">
        <xsl:with-param name="code" select="$functionCode" />
        <xsl:with-param name="type" select="'vars'" />
        <xsl:with-param name="delim" select="'&#x0A;'" />
      </xsl:call-template>
    </xsl:variable>
    <xsl:variable name="codeLines">
      <xsl:call-template name="processCode">
        <xsl:with-param name="code" select="$functionCode" />
        <xsl:with-param name="type" select="'code'" />
        <xsl:with-param name="delim" select="'&#x0A;'" />
      </xsl:call-template>
    </xsl:variable>
    <xsl:variable name="varLookupTable">
      <xsl:variable name="locals">
        <xsl:analyze-string select="substring-after($varDeclLine, 'var')" regex="([A-Za-z_][A-Za-z0-9_]*)(\s*=(\s+|[^;=/,&#x22;]+?|&#x22;[^&#x22;\n\r]*?&#x22;|\(([^;=,&#x22;]*?,?(&#x22;[^\n\r&#x22;]*?&#x22;)?)+\)|/[^/\n]+?/)+)">
          <xsl:matching-substring>
            <xsl:element name="Entry">
              <xsl:attribute name="type" select="'local'" />
              <xsl:element name="OrigName">
                <xsl:value-of select="regex-group(1)" />
              </xsl:element>
              <xsl:element name="Assign">
                <xsl:value-of select="regex-group(2)" />
              </xsl:element>
            </xsl:element>
          </xsl:matching-substring>
        </xsl:analyze-string>
      </xsl:variable>
      <xsl:for-each select="$locals/Entry">
        <xsl:element name="Entry">
          <xsl:attribute name="type" select="'local'" />
          <xsl:element name="OrigName">
            <xsl:value-of select="OrigName" />
          </xsl:element>
          <xsl:element name="NewName">
            <xsl:value-of select="concat('_l.v', position())" />
          </xsl:element>
          <xsl:element name="Assign">
            <xsl:value-of select="Assign" />
          </xsl:element>
        </xsl:element>
      </xsl:for-each>
      <xsl:for-each select="$params/Param">
        <xsl:element name="Entry">
          <xsl:attribute name="type" select="'param'" />
          <xsl:element name="OrigName">
            <xsl:value-of select="." />
          </xsl:element>
          <xsl:element name="NewName">
            <xsl:value-of select="concat('_l._p[', position() - 1, ']')" />
          </xsl:element>
        </xsl:element>
      </xsl:for-each>
    </xsl:variable>
    <xsl:for-each select="$varLookupTable/Entry[(@type eq 'local') and (string-length(Assign) gt 0)]">
      <xsl:element name="Code">
        <xsl:value-of select="concat(NewName, Assign, ';')"/>
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
    <xsl:param name="functionCode" />
    <xsl:param name="params" />
    <xsl:variable name="processedCode">
      <xsl:call-template name="mungeFunction">
        <xsl:with-param name="functionCode" select="$functionCode" />
        <xsl:with-param name="params" select="$params" />
      </xsl:call-template>
    </xsl:variable>
    <xsl:variable name="lineDelims">
      <xsl:element name="TermExpression">
        <xsl:attribute name="type" select="'Return'" />
        <xsl:attribute name="openCount" select="xs:integer(0)" />
        <xsl:text>\s+return\s+?.+?;$</xsl:text>
      </xsl:element>
      <xsl:element name="TermExpression">
        <xsl:attribute name="type" select="'Return'" />
        <xsl:attribute name="openCount" select="xs:integer(0)" />
        <xsl:text>^return\s+?.+?;$</xsl:text>
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
      <xsl:variable name="delimRegEx" select="concat('(', string-join($lineDelims/TermExpression, '|'), ')')" />
      <xsl:for-each select="$processedCode/Code" >
        <xsl:analyze-string select="." regex="{$delimRegEx}">
          <xsl:matching-substring>
            <xsl:element name="CodeDelim">
              <xsl:variable name="term" select="$lineDelims/TermExpression[matches(regex-group(1), .)][1]" />
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
        <xsl:variable name="code">
          <xsl:if test="count(preceding-sibling::CodePart) gt 0">
            <xsl:value-of select="preceding-sibling::CodePart[1]" />
          </xsl:if>
        </xsl:variable>
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
              <xsl:if test="string-length($code) gt 0">
                <xsl:value-of select="concat($code, ';')" />
              </xsl:if>
            </xsl:when>
            <xsl:when test="$delim/@DelimType eq 'Return'">
              <xsl:attribute name="BlockID" select="'-1'" />
              <xsl:attribute name="CodeType" select="'Return'" />
              <xsl:attribute name="ReturnedVal" select="normalize-space(substring-after($delim, 'return'))" />
            </xsl:when>
            <xsl:when test="$delim/@DelimType eq 'Else'">
              <xsl:attribute name="BlockID" select="'-1'" />
              <xsl:attribute name="ParentType" select="'Else'" />
              <xsl:attribute name="CodeType" select="'Parent'" />
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
    <xsl:variable name="sequences">
      <xsl:if test="count($functionXML/Code) eq 1">
        <xsl:element name="CodeSequence">
          <xsl:variable name="startPos" select="1" />
          <xsl:attribute name="Depth" select="0" />
          <xsl:attribute name="numFollowing" select="0" />
          <xsl:attribute name="Length" select="1" />
          <xsl:attribute name="SequenceNum" select="1" />
          <xsl:copy-of select="$functionXML/Code" />
        </xsl:element>
      </xsl:if>
      <xsl:if test="count($functionXML/Code) gt 1">
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
      </xsl:if>
    </xsl:variable>
    <xsl:variable name="formattedSequences">
      <xsl:for-each select="$sequences/CodeSequence">
        <xsl:variable name="thisSequence" select="." />
        <xsl:element name="CodeSequence">
          <xsl:attribute name="Depth" select="@Depth" />
          <xsl:attribute name="Position" select="position()" />
          <xsl:attribute name="OpenRole" select="Code[1]/@CodeType" />
          <xsl:attribute name="CloseRole" select="Code[last()]/@CodeType" />
          <xsl:attribute name="ContainsReturn" select="if (some $c in Code satisfies $c/@CodeType eq 'Return') then 'yes' else 'no'" />
          <xsl:for-each select="$thisSequence/Code">
            <xsl:if test="@CodeType eq 'Line'">
              <xsl:element name="Line">
                <xsl:if test="string-length(.) gt 0">
                  <xsl:value-of select="." />
                </xsl:if>
                <xsl:if test="string-length(.) eq 0">
                  <xsl:value-of select="';'" />
                </xsl:if>
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
      <xsl:for-each select="$formattedSequences/CodeSequence">
        <xsl:element name="Depth">
          <xsl:value-of select="xs:integer(@Depth)" />
        </xsl:element>
      </xsl:for-each>
    </xsl:variable>
    <xsl:variable name="maxDepth" select="max($depths/Depth)" />
    <xsl:if test="xs:integer($maxDepth) eq 0">
      <xsl:call-template name="ConstructSubFunction">
        <xsl:with-param name="functionSegments" select="$formattedSequences" />
        <xsl:with-param name="params" select="$ParamTable/ParamEntry/NewParam" />
        <xsl:with-param name="functName" select="$functionName" />
        <xsl:with-param name="segNum" select="0" />
      </xsl:call-template>
    </xsl:if>
    <xsl:if test="xs:integer($maxDepth) gt 0">
      <xsl:for-each select="0 to xs:integer($maxDepth)">
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
          <xsl:for-each select="$thisDepthSequences/newSeqNdx[xs:integer(preceding-sibling::newSeqNdx[1]) eq -1][xs:integer(.) ne -1]">
            <xsl:variable name="startCodePos" select="xs:integer(.)" />
            <xsl:variable name="endCodePos">
              <xsl:if test="position() eq last()">
                <xsl:value-of select="max($thisDepthSequences/newSeqNdx)"/>
              </xsl:if>
              <xsl:if test="position() lt last()">
                <xsl:value-of select="$thisDepthSequences/newSeqNdx[position() ge $startCodePos][(xs:integer(.) ne -1) and (xs:integer(following-sibling::newSeqNdx[1]) eq -1)][1]" />
              </xsl:if>
            </xsl:variable>
            <xsl:variable name="subSeqs">
              <xsl:for-each select="for $i in xs:integer($startCodePos) to xs:integer($endCodePos) return $i">
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
    </xsl:if>
  </xsl:template>

  <xsl:template name="ConstructSubFunction">
    <xsl:param name="functionSegments" />
    <xsl:param name="params" />
    <xsl:param name="numLocals" />
    <xsl:param name="functName" />
    <xsl:param name="segNum" />
    <xsl:variable name="containsReturn" select="if (some $cs in $functionSegments/CodeSequence satisfies $cs/@ContainsReturn eq 'yes') then 'yes' else 'no'" />
    <xsl:variable name="parentDepth" select="min(for $i in 1 to count($functionSegments/CodeSequence) return $functionSegments/CodeSequence[$i]/@Depth)" />
    <xsl:variable name="thisDepthSegNdxs">
      <xsl:for-each select="$functionSegments/CodeSequence">
        <xsl:if test="(xs:integer(@Depth) eq $parentDepth)">
          <xsl:element name="Ndx">
            <xsl:value-of select="position()" />
          </xsl:element>
        </xsl:if>
      </xsl:for-each>
    </xsl:variable>
    <xsl:variable name="segItrVals">
      <xsl:value-of select="(1 to count($functionSegments/CodeSequence))" />
    </xsl:variable>
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
      <xsl:for-each select="$thisDepthSegNdxs/Ndx">
        <xsl:variable name="segPosNdx" select="position()" />
        <xsl:variable name="segPos" select="xs:integer(.)" />
        <xsl:if test="$segPosNdx eq 1">
          <xsl:if test="$parentDepth eq 0">
            <xsl:element name="Line">
              <xsl:value-of select="'var _l = new Object();'"/>
            </xsl:element>
            <xsl:if test="($containsReturn eq 'yes') and (some $cs in $functionSegments/CodeSequence satisfies xs:integer($cs/@Depth) gt 0)">
              <xsl:element name="Line">
                <xsl:value-of select="'_l._hr = false;'" />
              </xsl:element>
              <xsl:element name="Line">
                <xsl:value-of select="'_l._rv = null;'" />
              </xsl:element>
            </xsl:if>
            <xsl:if test="count($params) gt 0">
              <xsl:element name="Line">
                <xsl:value-of select="'_l._p = _p;'" />
              </xsl:element>
            </xsl:if>
          </xsl:if>
        </xsl:if>
        <xsl:copy-of select="$functionSegments/CodeSequence[$segPos]/Line" />
        <xsl:if test="xs:integer($segPos) ne ($thisDepthSegNdxs/Ndx[$segPosNdx + 1] - 1)">
          <xsl:variable name="subDepth" select="$parentDepth + 1" />
          <xsl:variable name="subFunctName" select="concat($functName, '.s', $subDepth, '_', $segPos - $segPosNdx + 1)" />
          <xsl:element name="Line">
            <xsl:value-of select="concat($subFunctName, '.eval(_l);')" />
          </xsl:element>
          <xsl:if test="$containsReturn eq 'yes'">
            <xsl:element name="Line">
              <xsl:value-of select="'if (_l._hr == true) return _l._rv;'"/>
            </xsl:element>
          </xsl:if>
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
        <xsl:when test="$elem/@CodeType eq 'Return'">
          <xsl:value-of select="'Return'" />
        </xsl:when>
      </xsl:choose>
    </xsl:variable>
    <xsl:choose>
      <xsl:when test="$elemType eq 'Else'">
        <xsl:element name="Line">
          <xsl:value-of select="'else'" />
        </xsl:element>
      </xsl:when>
      <xsl:when test="$elemType eq 'for'">
        <xsl:element name="Line">
          <xsl:value-of select="concat('for (', (if (@VarDeclared) then 'var' else ''), @Var, ' = ', @StartValue, '; ', @Comparison, '; ', @VarChange, ')', (if ($elem/@CodeType eq 'OpenBlock') then ' {' else ''))" />
        </xsl:element>
      </xsl:when>
      <xsl:when test="$elemType eq 'while'">
        <xsl:element name="Line">
          <xsl:value-of select="concat('while (', @Condition, ')', (if (@CodeType eq 'OpenBlock') then '{' else ''))" />
        </xsl:element>
      </xsl:when>
      <xsl:when test="$elemType eq 'do'">
        <xsl:element name="Line">
          <xsl:value-of select="'do {'" />
        </xsl:element>
      </xsl:when>
      <xsl:when test="$elemType eq 'DoWhile'">
        <xsl:element name="Line">
          <xsl:value-of select="concat('while (', @Condition, ');')" />
        </xsl:element>
      </xsl:when>
      <xsl:when test="$elemType eq 'Term'">
        <xsl:element name="Line">
          <xsl:value-of select="'}'" />
        </xsl:element>
      </xsl:when>
      <xsl:when test="$elemType eq 'if'">
        <xsl:element name="Line">
          <xsl:value-of select="concat('if (', @Condition, ')', if ($elem/@CodeType eq 'OpenBlock') then '{' else '')" />
        </xsl:element>
      </xsl:when>
      <xsl:when test="$elemType eq 'none'">
        <xsl:if test="$elem/@CodeType eq 'OpenBlock'">
          <xsl:element name="Line">
            <xsl:value-of select="'{'" />
          </xsl:element>
        </xsl:if>
      </xsl:when>
      <xsl:when test="$elemType eq 'Return'">
        <xsl:element name="Line">
          <xsl:value-of select="'_l._hr = true;'" />
        </xsl:element>
        <xsl:element name="Line">
          <xsl:value-of select="concat('_l._rv = ', $elem/@ReturnedVal)" />
        </xsl:element>
        <xsl:element name="Line">
          <xsl:value-of select="'return _l._rv;'" />
        </xsl:element>
      </xsl:when>
    </xsl:choose>
  </xsl:template>
</xsl:stylesheet>