<?xml version="1.0" encoding="UTF-8"?>
<xsl:stylesheet xmlns:xs="http://www.w3.org/2001/XMLSchema"
                xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
                xmlns:mng="http://www.iatsoftware.net"
                version="2.0"
                exclude-result-prefixes="xs mng">

  <xsl:output method="xml" encoding="utf-8" indent="yes"
               cdata-section-elements="Code Line Function Declaration FunctionConstructor"/>

  <xsl:variable name="documentElement">
    <xsl:value-of select="//CodeFile" />
  </xsl:variable>

  <xsl:variable name="globalVars">
    <xsl:copy-of select="//VarEntries/Entry" />
  </xsl:variable>

  <xsl:variable name="globalCode">
    <xsl:element name="Function">
      <xsl:attribute name="FunctionNdx" select="'0'" />
      <xsl:attribute name="FunctionPrefix" select="'_gc'" />
      <xsl:attribute name="FunctionName" select="'GlobalCodeFunct'" />
      <xsl:element name="Params" />
      <xsl:element name="FunctionBody">
        <xsl:copy-of select="//GlobalCode" />
      </xsl:element>
    </xsl:element>
  </xsl:variable>
  <!--
  <xsl:template match="//GlobalCode">
    <xsl:apply-templates select="$globalCode/Function" />
  </xsl:template>
-->

  <xsl:template match="//VarEntries">
    <xsl:element name="GlobalVars">
      <xsl:element name="GlobalDecl">
        <xsl:if test="count(Entry) gt 0">
          <xsl:value-of select="'var '"/>
          <xsl:for-each select="Entry">
            <xsl:if test="string-length(normalize-space(Assign)) eq 0">
              <xsl:value-of select="concat(NewName, if (position() eq last()) then ';' else ',')" />
            </xsl:if>
            <xsl:if test="string-length(normalize-space(Assign)) ne 0">
              <xsl:value-of select="concat(NewName, '=', Assign, if (position() eq last()) then ';' else ',')" />
            </xsl:if>
          </xsl:for-each>
        </xsl:if>
      </xsl:element>
      <xsl:element name="GlobalVarNameTable">
        <xsl:for-each select="Entry">
          <xsl:element name="VarTableEntry">
            <xsl:element name="OrigName">
              <xsl:value-of select="OrigName" />
            </xsl:element>
            <xsl:element name="NewName">
              <xsl:value-of select="NewName" />
            </xsl:element>
          </xsl:element>
        </xsl:for-each>
      </xsl:element>
    </xsl:element>
  </xsl:template>

  <xsl:template match="//Class">
    <xsl:variable name="class" select="."/>
    <xsl:variable name="classPrefix" select="@ClassPrefix" />
    <xsl:variable name="classFunctionPrefix" select="@ClassFunctionPrefix" />
    <xsl:variable name="classNdx" select="count(//Classes/Class[@ClassName eq $class/@ClassName]/preceding-sibling::Class) + 1" />
    <xsl:variable name="className" select="concat($classPrefix, $classNdx)" />
    <xsl:variable name="classCode">
      <xsl:element name="Constructor">
        <xsl:copy-of select="Constructor/Params"/>
        <xsl:call-template name="ConstructFunction">
          <xsl:with-param name="functionName" select="concat($classPrefix, $classNdx)"/>
          <xsl:with-param name="type" select="'constructor'" />
          <xsl:with-param name="functionCode" select="Constructor/ConstructorBody/Code"/>
          <xsl:with-param name="params" select="Constructor/Params"/>
        </xsl:call-template>
      </xsl:element>
      <xsl:element name="PrototypeChain">
        <xsl:for-each select="PrototypeChain/Function">
          <xsl:element name="Function">
            <xsl:attribute name="Name" select="@FunctionName"/>
            <xsl:copy-of select="Params"/>
            <xsl:call-template name="ConstructFunction">
              <xsl:with-param name="functionName" select="concat($classPrefix, $classNdx, '.', $classFunctionPrefix, position())"/>
              <xsl:with-param name="type" select="'memeberFunction'" />
              <xsl:with-param name="functionCode" select="FunctionBody/Code"/>
              <xsl:with-param name="params" select="Params"/>
            </xsl:call-template>
          </xsl:element>
        </xsl:for-each>
      </xsl:element>
    </xsl:variable>
    <xsl:element name="ProcessedCode">
      <xsl:attribute name="EntityName" select="@ClassName"/>
      <xsl:attribute name="Name" select="concat($classPrefix, $classNdx)"/>
      <xsl:variable name="classDecl">
        <xsl:variable name="conParams">
          <xsl:value-of select="string-join(Constructor/Params/Param, ', ')"/>
        </xsl:variable>
        <xsl:element name="Part">
          <xsl:if test="empty($conParams)">
            <xsl:value-of select="concat('function ', @ClassName, '(', $conParams, ') { ', $classPrefix, $classNdx, '.cEval(this, new Array()); return this;} ')"/>
          </xsl:if>
          <xsl:if test="not(empty($conParams))">
            <xsl:value-of select="concat('function ', @ClassName, '(', $conParams, ') { ', $classPrefix, $classNdx, '.cEval(this, [', $conParams, ']); return this; } ')"/>
          </xsl:if>
        </xsl:element>
        <xsl:if test="Super/@Has eq 'yes'">
          <xsl:element name="Part">
            <xsl:value-of select="concat(@ClassName, '.prototype = new ', Super, '(); ')"/>
          </xsl:element>
        </xsl:if>
        <xsl:element name="Part">
          <xsl:value-of select="concat($class/@ClassName, '.prototype.constructor = ', $class/@ClassName, '; ')" />
        </xsl:element>
        <xsl:for-each select="PrototypeChain/Function">
          <xsl:variable name="funParams">
            <xsl:value-of select="string-join(Params/Param, ', ')"/>
          </xsl:variable>
          <xsl:element name="Part">
            <xsl:if test="empty(Params)">
              <xsl:value-of select="concat($class/@ClassName, '.prototype.', @FunctionName, ' = function(', $funParams, ') { return ', $classPrefix, $classNdx, '.', $classFunctionPrefix, position(), '.cEval(this, new Array()); }; ')"/>
            </xsl:if>
            <xsl:if test="not(empty(Params))">
              <xsl:value-of select="concat($class/@ClassName, '.prototype.', @FunctionName, ' = function(', $funParams, ') { return ', $classPrefix, $classNdx, '.', $classFunctionPrefix, position(), '.cEval(this, [ ', $funParams, ' ]); }; ')"/>
            </xsl:if>
          </xsl:element>
        </xsl:for-each>
      </xsl:variable>
      <xsl:variable name="subFuncts">
        <xsl:for-each select="$classCode//SubFunction">
          <xsl:element name="Function">
            <xsl:attribute name="Name" select="@FunctionName"/>
            <xsl:attribute name="SubParam" select="@SubParam"/>
            <xsl:copy-of select="Line"/>
          </xsl:element>
        </xsl:for-each>
      </xsl:variable>
      <xsl:call-template name="buildFunctDescriptor">
        <xsl:with-param name="entityType" select="'class'" />
        <xsl:with-param name="source" select="$class" />
        <xsl:with-param name="functions" select="$subFuncts/Function/@Name" />
        <xsl:with-param name="entityNdx" select="$classNdx" />
      </xsl:call-template>
      <xsl:element name="Declaration">
        <xsl:value-of select="string-join($classDecl, ' ')" />
      </xsl:element>
      <xsl:variable name="memberVariables">
        <xsl:for-each select="$subFuncts//Line">
          <xsl:analyze-string select="." regex="^this\.([A-Za-z_][A-Za-z0-9_]*)">
            <xsl:matching-substring>
              <xsl:if test="every $mf in $classCode/PrototypeChain/Function satisfies $mf/@Name ne regex-group(1)">
                <xsl:element name="MemberVariable">
                  <xsl:value-of select="concat('this.', regex-group(1))"/>
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
              <xsl:value-of select="."/>
            </xsl:element>
            <xsl:element name="NewDecl">
              <xsl:value-of select="concat('this._c', $classNdx, '_mv', position())"/>
            </xsl:element>
          </xsl:element>
        </xsl:for-each>
      </xsl:variable>
      <xsl:variable name="mvRegEx"
                    select="concat('(^|[^A-Za-z0-9_\.])(', string-join($mvTable/Entry/OrigDecl, '|'), ')([^A-Za-z0-9_])')"/>
      <xsl:element name="Functions">
        <xsl:for-each select="$subFuncts/Function">
          <xsl:element name="Function">
            <xsl:attribute name="Name" select="@Name"/>
            <xsl:attribute name="Param" select="@SubParam"/>
            <xsl:analyze-string select="string-join(Line, ' ')" regex="{$mvRegEx}">
              <xsl:matching-substring>
                <xsl:value-of select="concat(regex-group(1), $mvTable/Entry[OrigDecl eq regex-group(2)]/NewDecl, regex-group(3))"/>
              </xsl:matching-substring>
              <xsl:non-matching-substring>
                <xsl:value-of select="."/>
              </xsl:non-matching-substring>
            </xsl:analyze-string>
          </xsl:element>
        </xsl:for-each>
      </xsl:element>
    </xsl:element>
  </xsl:template>

  <xsl:template match="//Function">
    <xsl:variable name="functionPrefix" select="@FunctionPrefix" />
    <xsl:variable name="thisFunct" select="." />
    <xsl:variable name="functNdx" select="count(//Functions/Function[@FunctionName eq $thisFunct/@FunctionName]/preceding-sibling::Function) + 1"/>
    <xsl:variable name="function">
      <xsl:copy-of select="Params"/>
      <xsl:call-template name="ConstructFunction">
        <xsl:with-param name="functionName" select="concat($functionPrefix, $functNdx)"/>
        <xsl:with-param name="type" select="'function'" />
        <xsl:with-param name="functionCode" select="FunctionBody/Code"/>
        <xsl:with-param name="params" select="Params"/>
      </xsl:call-template>
    </xsl:variable>
    <xsl:element name="ProcessedCode">
      <xsl:attribute name="EntityName" select="@FunctionName"/>
      <xsl:attribute name="Name" select="concat($functionPrefix, $functNdx)"/>
      <xsl:call-template name="buildFunctDescriptor">
        <xsl:with-param name="entityType" select="'function'" />
        <xsl:with-param name="source" select="." />
        <xsl:with-param name="functions" select="$function//SubFunction/@FunctionName" />
        <xsl:with-param name="entityNdx" select="$functNdx" />
      </xsl:call-template>
      <xsl:element name="Declaration">
        <xsl:variable name="params" select="string-join($function/Params/Param, ', ')"/>
        <xsl:value-of select="concat('function ', @FunctionName, '(', $params, ') { return ', $functionPrefix, $functNdx, '.fEval([', $params, ']); }')"/>
      </xsl:element>
      <xsl:element name="Functions">
        <xsl:for-each select="$function//SubFunction">
          <xsl:element name="Function">
            <xsl:attribute name="Name" select="@FunctionName"/>
            <xsl:attribute name="Param" select="@SubParam"/>
            <xsl:value-of select="string-join(Line, ' ')"/>
          </xsl:element>
        </xsl:for-each>
      </xsl:element>
    </xsl:element>
  </xsl:template>

  <xsl:template match="//GlobalCode">
    <xsl:variable name="function">
      <xsl:element name="Params" />
      <xsl:variable name="params">
        <xsl:element name="Params" />
      </xsl:variable>
      <xsl:call-template name="ConstructFunction">
        <xsl:with-param name="functionName" select="@CodePrefix"/>
        <xsl:with-param name="type" select="'function'" />
        <xsl:with-param name="functionCode" select="Code"/>
        <xsl:with-param name="params" select="$params"/>
      </xsl:call-template>
    </xsl:variable>
    <xsl:element name="ProcessedCode">
      <xsl:attribute name="Name" select="@CodePrefix"/>
      <xsl:call-template name="buildFunctDescriptor">
        <xsl:with-param name="entityType" select="'function'" />
        <xsl:with-param name="source" select="." />
        <xsl:with-param name="functions" select="$function//SubFunction/@FunctionName" />
        <xsl:with-param name="entityNdx" select="'0'" />
      </xsl:call-template>
      <xsl:element name="Declaration">
        <xsl:value-of select="concat('function ', @CodePrefix, '_globalfunction() { return ', @CodePrefix, '.fEval([]); }')"/>
      </xsl:element>
      <xsl:element name="Functions">
        <xsl:for-each select="$function//SubFunction">
          <xsl:element name="Function">
            <xsl:attribute name="Name" select="@FunctionName"/>
            <xsl:attribute name="Param" select="@SubParam"/>
            <xsl:value-of select="string-join(Line, ' ')"/>
          </xsl:element>
        </xsl:for-each>
      </xsl:element>
    </xsl:element>
  </xsl:template>

  <xsl:template match="CodeFile">
    <xsl:element name="CodeFile">
      <xsl:apply-templates select="//VarEntries" />
      <xsl:variable name="globalFuncts">
        <xsl:apply-templates select="//Functions/Function" />
      </xsl:variable>
      <xsl:variable name="globalNameTable">
        <xsl:for-each select="//VarEntries/Entry">
          <xsl:element name="Entry">
            <xsl:element name="OldName">
              <xsl:value-of select="OrigName" />
            </xsl:element>
            <xsl:element name="NewName">
              <xsl:value-of select="NewName" />
            </xsl:element>
          </xsl:element>
        </xsl:for-each>
      </xsl:variable>
      <xsl:for-each select="$globalFuncts/ProcessedCode">
        <xsl:call-template name="replaceGlobals">
          <xsl:with-param name="globalLookupTable" select="$globalNameTable" />
          <xsl:with-param name="functionElem" select="." />
        </xsl:call-template>
      </xsl:for-each>
      <xsl:variable name="Classes">
        <xsl:apply-templates select="//Class" />
      </xsl:variable>
      <xsl:for-each select="$Classes/ProcessedCode">
        <xsl:call-template name="replaceGlobals">
          <xsl:with-param name="globalLookupTable" select="$globalNameTable" />
          <xsl:with-param name="functionElem" select="." />
        </xsl:call-template>
      </xsl:for-each>
      <xsl:if test="count(//GlobalCode/Code) ge 1">
        <xsl:variable name="GlobalCode">
          <xsl:apply-templates select="//GlobalCode" />
        </xsl:variable>
        <xsl:for-each select="$GlobalCode/ProcessedCode">
          <xsl:call-template name="replaceGlobals">
            <xsl:with-param name="globalLookupTable" select="$globalNameTable" />
            <xsl:with-param name="functionElem" select="." />
          </xsl:call-template>
        </xsl:for-each>
        <xsl:element name="GlobalCode">
          <xsl:element name="Code">
            <xsl:value-of select="concat(//GlobalCode/@CodePrefix, '_globalfunction();')" />
          </xsl:element>
        </xsl:element>
      </xsl:if>
    </xsl:element>
  </xsl:template>

  <xsl:template name="replaceGlobals">
    <xsl:param name="globalLookupTable" />
    <xsl:param name="functionElem" />
    <xsl:variable name="varRegEx" select="concat('((([^A-Za-z0-9\.]|^)(', string-join($globalLookupTable/Entry/OldName, '|'), ')([^A-Za-z0-9_]|$))|([^\\]?&#x22;))')" />
    <xsl:variable name="NewFunctions">
      <xsl:for-each select="$functionElem/Functions/Function">
        <xsl:element name="Function">
          <xsl:attribute name="Name" select="@Name" />
          <xsl:attribute name="Param" select="@Param" />
          <xsl:if test="count($globalLookupTable/Entry) gt 0">
            <xsl:variable name="tokenizedCode">
              <xsl:analyze-string select="." regex="{$varRegEx}">
                <xsl:matching-substring>
                  <xsl:if test="string-length(regex-group(2)) gt 0">
                    <xsl:element name="codePart">
                      <xsl:if test="string-length(regex-group(3)) gt 0">
                        <xsl:attribute name="type" select="'code'" />
                      </xsl:if>
                      <xsl:if test="string-length(regex-group(3)) eq 0">
                        <xsl:attribute name="type" select="'empty'" />
                      </xsl:if>
                      <xsl:value-of select="regex-group(3)" />
                    </xsl:element>
                    <xsl:element name="codePart">
                      <xsl:attribute name="type" select="'potentialVar'" />
                      <xsl:value-of select="regex-group(4)" />
                    </xsl:element>
                    <xsl:element name="codePart">
                      <xsl:if test="string-length(regex-group(5)) gt 0">
                        <xsl:attribute name="type" select="'code'" />
                      </xsl:if>
                      <xsl:if test="string-length(regex-group(5)) eq 0">
                        <xsl:attribute name="type" select="'empty'" />
                      </xsl:if>
                      <xsl:value-of select="regex-group(5)" />
                    </xsl:element>
                  </xsl:if>
                  <xsl:if test="string-length(regex-group(6)) gt 0">
                    <xsl:if test="string-length(regex-group(6)) gt 1">
                      <xsl:element name="codePart">
                        <xsl:attribute name="type" select="'code'" />
                        <xsl:value-of select="substring(regex-group(6), 1, 1)" />
                      </xsl:element>
                    </xsl:if>
                    <xsl:element name="codePart">
                      <xsl:attribute name="type" select="'quote'" />
                      <xsl:value-of select="'&#x22;'" />
                    </xsl:element>
                  </xsl:if>
                </xsl:matching-substring>
                <xsl:non-matching-substring>
                  <xsl:element name="codePart">
                    <xsl:attribute name="type" select="'code'" />
                    <xsl:value-of select="." />
                  </xsl:element>
                </xsl:non-matching-substring>
              </xsl:analyze-string>
            </xsl:variable>
            <xsl:for-each select="$tokenizedCode/codePart">
              <xsl:if test="@type eq 'potentialVar'">
                <xsl:variable name="potVarName" select="." />
                <xsl:if test="position() eq 1">
                  <xsl:if test="matches(substring(following-sibling::codePart[1], 1, 1), '^[^A-Za-z0-9_]')">
                    <xsl:value-of select="$globalLookupTable/Entry[OldName eq $potVarName]/NewName" />
                  </xsl:if>
                  <xsl:if test="not(matches(substring(following-sibling::codePart[1], 1, 1), '^[^A-Za-z0-9_]'))">
                    <xsl:value-of select="." />
                  </xsl:if>
                </xsl:if>
                <xsl:if test="position() eq last()">
                  <xsl:if test="matches(substring(preceding-sibling::codePart[1], string-length(preceding-sibling::codePart[1]) - 1), '[^A-Za-z_\.]$')">
                    <xsl:value-of select="$globalLookupTable/Entry[OldName eq $potVarName]/NewName" />
                  </xsl:if>
                  <xsl:if test="not(matches(substring(preceding-sibling::codePart[1], string-length(preceding-sibling::codePart[1]) - 1), '[^A-Za-z_\.]$'))">
                    <xsl:value-of select="." />
                  </xsl:if>
                </xsl:if>
                <xsl:if test="(position() ne 1) and (position() ne last())">
                  <xsl:if test="(count(preceding-sibling::codePart[@type eq 'quote']) mod 2) eq 0">
                    <xsl:if test="(matches(substring(preceding-sibling::codePart[1], string-length(preceding-sibling::codePart[1]) - 1), '[^A-Za-z_\.]$')) or (preceding-sibling::codePart[1]/@type eq 'empty')">
                      <xsl:if test="(matches(substring(following-sibling::codePart[1], 1, 1), '^[^A-Za-z0-9_]')) or (following-sibling::codePart[1]/@type eq 'empty')">
                        <xsl:value-of select="$globalLookupTable/Entry[OldName eq $potVarName]/NewName" />
                      </xsl:if>
                    </xsl:if>
                  </xsl:if>
                  <xsl:if test="(count(preceding-sibling::codePart[@type eq 'quote']) mod 2) eq 1">
                    <xsl:value-of select="." />
                  </xsl:if>
                </xsl:if>
                <xsl:if test="not((matches(substring(preceding-sibling::codePart[1], string-length(preceding-sibling::codePart[1]) - 1), '[^A-Za-z_\.]$')) or (preceding-sibling::codePart[1]/@type eq 'empty'))">
                  <xsl:if test="not((matches(substring(following-sibling::codePart[1], 1, 1), '^[^A-Za-z0-9_]')) or (following-sibling::codePart[1]/@type eq 'empty'))">
                    <xsl:value-of select="." />
                  </xsl:if>
                </xsl:if>
              </xsl:if>
              <xsl:if test="@type eq 'quote'">
                <xsl:value-of select="." />
              </xsl:if>
              <xsl:if test="@type eq 'code'">
                <xsl:value-of select="." />
              </xsl:if>
            </xsl:for-each>
          </xsl:if>
          <xsl:if test="count($globalLookupTable/Entry) eq 0">
            <xsl:value-of select="." />
          </xsl:if>
        </xsl:element>
      </xsl:for-each>
    </xsl:variable>
    <xsl:element name="ProcessedCode">
      <xsl:attribute name="EntityName">
        <xsl:value-of select="@Name" />
      </xsl:attribute>
      <xsl:copy-of select="Declaration" />
      <xsl:element name="Functions">
        <xsl:copy-of select="$NewFunctions/Function" />
      </xsl:element>
    </xsl:element>
  </xsl:template>

  <xsl:template name="processCode">
    <xsl:param name="code"/>
    <xsl:param name="type"/>
    <xsl:param name="delim"/>
    <xsl:variable name="codeList">
      <xsl:for-each select="$code">
        <xsl:variable name="line" select="normalize-space(replace(., '(.+?)$', '$1'))"/>
        <xsl:choose>
          <xsl:when test="matches($line, '^var(\s+)?([A-Za-z_][A-Za-z0-9_]*)(\s*=\s*(\[|\s+|[^,;=/&#34;\(\[\]]+|(&#34;[^&#xA;&#xD;&#34;]*?&#34;)+|\(([^;,=&#34;]*(,)?(&#34;[^&#xA;&#xD;&#34;]*?&#34;)?)*?\)|/[^/\n]+?/|\](\s*,)?)+)?')">
            <xsl:analyze-string select="replace($line, '(var\s+)(.+)', '$2')"
                                regex="([A-Za-z_][A-Za-z0-9_]*)(\s*=\s*(\[|\s+|[^,;=/&#34;\(\[\]]+|(&#34;[^&#xA;&#xD;&#34;]*?&#34;)+|\(([^;,=&#34;]*(,)?(&#34;[^&#xA;&#xD;&#34;]*?&#34;)?)*?\)|/[^/\n]+?/|\](\s*,)?)+)?">
              <xsl:matching-substring>
                <xsl:element name="code">
                  <xsl:attribute name="type" select="'varName'"/>
                  <xsl:value-of select="regex-group(1)"/>
                </xsl:element>
                <xsl:if test="string-length(regex-group(2)) gt 0">
                  <xsl:element name="code">
                    <xsl:attribute name="type" select="'varAssign'"/>
                    <xsl:value-of select="concat(regex-group(2), ';')"/>
                  </xsl:element>
                </xsl:if>
              </xsl:matching-substring>
            </xsl:analyze-string>
          </xsl:when>
          <xsl:when test="matches($line, '(^|((&#34;.*?&#34;)*?[^&#34;]*?[^A-Za-z_\.\\|]))(var\s+)([A-Za-z_][A-Za-z0-9_]*)')">
            <xsl:analyze-string select="$line"
                                regex="(^|((&#34;.*?&#34;)*?[^&#34;]*?[^A-Za-z_\.\\|]))(var\s+)([A-Za-z_][A-Za-z0-9_]*)">
              <xsl:matching-substring>
                <xsl:element name="code">
                  <xsl:attribute name="type" select="'partialCode'"/>
                  <xsl:value-of select="regex-group(1)"/>
                </xsl:element>
                <xsl:element name="code">
                  <xsl:attribute name="type" select="'varName'"/>
                  <xsl:value-of select="regex-group(5)"/>
                </xsl:element>
              </xsl:matching-substring>
              <xsl:non-matching-substring>
                <xsl:element name="code">
                  <xsl:attribute name="type" select="'partialCode'"/>
                  <xsl:value-of select="."/>
                </xsl:element>
              </xsl:non-matching-substring>
            </xsl:analyze-string>
          </xsl:when>
          <xsl:otherwise>
            <xsl:element name="code">
              <xsl:attribute name="type" select="'code'"/>
              <xsl:value-of select="$line"/>
            </xsl:element>
          </xsl:otherwise>
        </xsl:choose>
        <xsl:element name="code">
          <xsl:attribute name="type" select="'delim'"/>
        </xsl:element>
      </xsl:for-each>
    </xsl:variable>
    <xsl:if test="(($type eq 'vars') or ($type eq 'both')) and (count($codeList/code[@type eq 'varName']) gt 0)">
      <xsl:for-each select="$codeList/code[((@type eq 'varName') and (every $var in preceding-sibling::code[@type eq 'varName'] satisfies normalize-space(.) ne normalize-space($var))) or (@type eq 'varAssign')]">
        <xsl:element name="VarDecl">
          <xsl:variable name="varName" select="."/>
          <xsl:if test="@type eq 'varName'">
            <xsl:choose>
              <xsl:when test="(position() eq last()) and (every $var in preceding-sibling::code[@type eq 'varName'] satisfies $var ne $varName)">
                <xsl:value-of select="."/>
              </xsl:when>
              <xsl:when test="(count(following-sibling::code[@type eq 'varName']) gt 0) and (following-sibling::code[1]/@type ne 'varAssign')">
                <xsl:value-of select="."/>
              </xsl:when>
              <xsl:when test="following-sibling::code[1]/@type eq 'varAssign'">
                <xsl:variable name="assign" select="following-sibling::code[1]"/>
                <xsl:choose>
                  <xsl:when test="matches($assign, '(^|[^A-Za-z_])this\.')">
                    <xsl:value-of select="."/>
                  </xsl:when>
                  <xsl:when test="some $var in preceding-sibling::code[@type eq 'varName'] satisfies (matches($assign, concat('[^A-Za-z0-9_]', normalize-space($var), '[^A-Za-z0-9_]?')) or matches($assign, concat('^', normalize-space($var), '[^A-Za-z0-9_]?')))">
                    <xsl:value-of select="."/>
                  </xsl:when>
                  <xsl:when test="(every $var in preceding-sibling::code[@type eq 'varName'] satisfies $var ne $varName) and (every $var in following-sibling::code[@type eq 'varName'] satisfies $var ne $varName)">
                    <xsl:if test="position() + 1 eq last()">
                      <xsl:value-of select="concat(., $assign)"/>
                    </xsl:if>
                    <xsl:if test="position() + 1 ne last()">
                      <xsl:value-of select="concat(., $assign)"/>
                    </xsl:if>
                  </xsl:when>
                  <xsl:otherwise>
                    <xsl:choose>
                      <xsl:when test="(position() + 1 eq last()) or (every $followingVar in following-sibling::code[@type eq 'varName'] satisfies ($followingVar eq $varName) or (some $precedingVar in preceding-sibling::code[@type eq 'varName'] satisfies $followingVar eq $precedingVar))">
                        <xsl:value-of select="."/>
                      </xsl:when>
                      <xsl:when test="position() + 1 ne last()">
                        <xsl:value-of select="."/>
                      </xsl:when>
                    </xsl:choose>
                  </xsl:otherwise>
                </xsl:choose>
              </xsl:when>
            </xsl:choose>
          </xsl:if>
        </xsl:element>
      </xsl:for-each>
    </xsl:if>
    <xsl:if test="($type eq 'code') or ($type eq 'both')">
      <xsl:for-each select="$codeList/code">
        <xsl:element name="Code">
          <xsl:choose>
            <xsl:when test="@type eq 'code'">
              <xsl:value-of select="."/>
            </xsl:when>
            <xsl:when test="(@type eq 'partialCode')">
              <xsl:variable name="prevCodeType">
                <xsl:if test="count(preceding-sibling::code) eq 0">
                  <xsl:value-of select="'code'" />
                </xsl:if>
                <xsl:if test="count(preceding-sibling::code) gt 0">
                  <xsl:value-of select="preceding-sibling::code[1]/@type" />
                </xsl:if>
              </xsl:variable>
              <xsl:variable name="currPCL" select="." />
              <xsl:if test="($prevCodeType eq 'code') or ($prevCodeType eq 'delim')">
                <xsl:value-of select="concat(., string-join($currPCL/following-sibling::code[every $n in 1 to position() satisfies empty(('code', 'delim')[. eq $currPCL/following-sibling::code[$n]/@type])], ' '))" />
              </xsl:if>
            </xsl:when>
            <xsl:when test="(@type eq 'varAssign') and (preceding-sibling::code[1]/@type eq 'varName')">
              <xsl:variable name="assign" select="normalize-space(.)"/>
              <xsl:variable name="thisVarName" select="preceding-sibling::code[@type eq 'varName'][1]" />
              <xsl:choose>
                <xsl:when test="matches($assign, '(^|[^A-Za-z_])this\.')">
                  <xsl:value-of select="concat($thisVarName, ' ', $assign, $delim)" />
                </xsl:when>
                <xsl:when test="some $var in preceding-sibling::code[@type eq 'varName'][position() gt 1] satisfies matches($assign, concat('[^A-Za-z0-9_]?', normalize-space($var), '[^A-Za-z0-9_]?'))">
                  <xsl:value-of select="concat(preceding-sibling::code[1], .)"/>
                </xsl:when>
                <xsl:when test="(some $var in preceding-sibling::code[@type eq 'varName'][position() gt 1] satisfies $var eq $thisVarName) or (some $var in following-sibling::code[@type eq 'varName'] satisfies $var eq $thisVarName)">
                  <xsl:value-of select="concat(preceding-sibling::code[1], .)"/>
                </xsl:when>
              </xsl:choose>
            </xsl:when>
            <xsl:when test="(@type eq 'varName') and (position() gt 1)">
              <xsl:if test="preceding-sibling::code[1]/@type eq 'code'">
                <xsl:value-of select="."/>
              </xsl:if>
            </xsl:when>
            <xsl:when test="@type eq 'subLineDelim'">
              <xsl:if test="matches(., '^[^;]')">
                <xsl:value-of select="concat(., $delim)"/>
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
                          <xsl:value-of select="concat(., $delim)"/>
                        </xsl:when>
                        <xsl:when test="(some $var in preceding-sibling::code[@type eq 'varName'][position() gt 1] satisfies $var eq $thisVarName) or (some $var in following-sibling::code[@type eq 'varName'] satisfies $var eq $thisVarName)">
                          <xsl:value-of select="concat(., $delim)"/>
                        </xsl:when>
                      </xsl:choose>
                    </xsl:if>
                  </xsl:when>
                  <xsl:when test="preceding-sibling::code[1]/@type eq 'varName'">
                    <xsl:variable name="varName" select="preceding-sibling::code[1]"/>
                    <xsl:if test="position() gt 2">
                      <xsl:if test="every $elem in preceding-sibling::code[@type eq 'varName'] satisfies $elem ne $varName">
                        <xsl:value-of select="concat(., $delim)"/>
                      </xsl:if>
                    </xsl:if>
                  </xsl:when>
                  <xsl:when test="preceding-sibling::code[1]/@type eq 'code'">
                    <xsl:value-of select="concat(., $delim)"/>
                  </xsl:when>
                </xsl:choose>
              </xsl:if>
            </xsl:when>
          </xsl:choose>
        </xsl:element>
      </xsl:for-each>
    </xsl:if>
  </xsl:template>

  <xsl:template name="mungeFunction">
    <xsl:param name="functionCode"/>
    <xsl:param name="params"/>
    <xsl:variable name="varDeclLine">
      <xsl:call-template name="processCode">
        <xsl:with-param name="code" select="$functionCode"/>
        <xsl:with-param name="type" select="'vars'"/>
        <xsl:with-param name="delim" select="'&#xA;'"/>
      </xsl:call-template>
    </xsl:variable>
    <xsl:variable name="codeLines">
      <xsl:call-template name="processCode">
        <xsl:with-param name="code" select="$functionCode"/>
        <xsl:with-param name="type" select="'code'"/>
        <xsl:with-param name="delim" select="'&#xA;'"/>
      </xsl:call-template>
    </xsl:variable>
    <xsl:variable name="varLookupTable">
      <xsl:variable name="locals">
        <xsl:for-each select="$varDeclLine/VarDecl">
          <xsl:analyze-string select="." regex="([A-Za-z_][A-Za-z0-9_]*)(\s*=.*)?;?">
            <xsl:matching-substring>
              <xsl:element name="Entry">
                <xsl:attribute name="type" select="'local'"/>
                <xsl:element name="OrigName">
                  <xsl:value-of select="regex-group(1)"/>
                </xsl:element>
                <xsl:element name="Assign">
                  <xsl:value-of select="regex-group(2)"/>
                </xsl:element>
              </xsl:element>
            </xsl:matching-substring>
          </xsl:analyze-string>
        </xsl:for-each>
      </xsl:variable>
      <xsl:for-each select="$locals/Entry">
        <xsl:element name="Entry">
          <xsl:attribute name="type" select="'local'"/>
          <xsl:element name="OrigName">
            <xsl:value-of select="OrigName"/>
          </xsl:element>
          <xsl:element name="NewName">
            <xsl:value-of select="concat('v', position())"/>
          </xsl:element>
          <xsl:element name="Assign">
            <xsl:value-of select="Assign"/>
          </xsl:element>
        </xsl:element>
      </xsl:for-each>
      <xsl:for-each select="$params/Param">
        <xsl:element name="Entry">
          <xsl:attribute name="type" select="'param'"/>
          <xsl:element name="OrigName">
            <xsl:value-of select="."/>
          </xsl:element>
          <xsl:element name="NewName">
            <xsl:value-of select="concat('_p[', position() - 1, ']')"/>
          </xsl:element>
        </xsl:element>
      </xsl:for-each>
    </xsl:variable>
    <xsl:if test="count($varLookupTable/Entry) gt 0">
      <xsl:for-each select="$varLookupTable/Entry[(@type eq 'local') and (string-length(Assign) gt 0)]">
        <xsl:element name="Code">
          <xsl:value-of select="mng:replaceVars($varLookupTable/Entry/OrigName, $varLookupTable/Entry/NewName, concat(OrigName, Assign))" />
        </xsl:element>
      </xsl:for-each>
      <xsl:for-each select="$codeLines/Code">
        <xsl:if test="string-length(normalize-space(.)) gt 0">
          <xsl:element name="Code">
            <xsl:value-of select="mng:replaceVars($varLookupTable/Entry/OrigName, $varLookupTable/Entry/NewName, .)" />
          </xsl:element>
        </xsl:if>
      </xsl:for-each>
    </xsl:if>
    <xsl:if test="count($varLookupTable/Entry) eq 0">
      <xsl:for-each select="$codeLines/Code">
        <xsl:element name="Code">
          <xsl:value-of select="." />
        </xsl:element>
      </xsl:for-each>
    </xsl:if>
  </xsl:template>

  <!--      <xsl:variable name="varRegEx"
                    select="concat('(|(((&#34;[^&#34;&#xA;&#xD;]*&#34;)*?[^&#34;&#xA;&#xD;]*?)?[^A-Za-z_\.\\|]))(', string-join($varLookupTable/Entry/OrigName, '|'), ')(([^A-Za-z0-9_]+?)|$)')"/>  
      <xsl:variable name="varRegEx" select="concat('(', string-join($varLookupTable/Entry/OrigName, '|'), ')')" />
      <xsl:copy-of select="$varRegEx" />
      <xsl:for-each select="$varLookupTable/Entry[(@type eq 'local') and (string-length(Assign) gt 0)]">
        <xsl:element name="Code">
          <xsl:variable name="assignElems">
            <xsl:analyze-string select="Assign" regex="{$varRegEx}">
              <xsl:matching-substring>
                <xsl:variable name="varName"
                              select="$varLookupTable/Entry[OrigName eq regex-group(5)]/NewName"/>
                <xsl:value-of select="concat(regex-group(1), '_l.', $varName, regex-group(6))"/>
              </xsl:matching-substring>
              <xsl:non-matching-substring>
                <xsl:value-of select="."/>
              </xsl:non-matching-substring>
            </xsl:analyze-string>   
            <xsl:variable name="processedLine">
              <xsl:element name=
              <xsl:value-of select="replace(Assign, $varRegEx, '$5')"/>
            <xsl:value-of select="replace(Assign, $varRegEx, concat('$1_l.', $varLookupTable/Entry[OrigName eq ]/NewName, '$6'))" />
          </xsl:variable>
          <xsl:value-of select="concat('_l.', NewName, $assign)"/>
        </xsl:element>
      </xsl:for-each>
      <xsl:for-each select="$codeLines/Code">
        <xsl:if test="string-length(normalize-space(.)) gt 0">
          <xsl:element name="Code">
            <xsl:analyze-string select="normalize-space(.)" regex="{$varRegEx}">
              <xsl:matching-substring>
                <xsl:variable name="varName"
                              select="$varLookupTable/Entry[OrigName eq regex-group(5)]/NewName"/>
                <xsl:value-of select="concat(regex-group(1), '_l.', $varName, regex-group(6))"/>
              </xsl:matching-substring>
              <xsl:non-matching-substring>
                <xsl:value-of select="."/>
              </xsl:non-matching-substring>
            </xsl:analyze-string>

          </xsl:element>
        </xsl:if>
      </xsl:for-each>
    </xsl:if>
    <xsl:if test="count($varLookupTable/Entry) eq 0">
      <xsl:for-each select="$codeLines/Code">
        <xsl:element name="Code">
          <xsl:value-of select="."/>
        </xsl:element>
      </xsl:for-each>
    </xsl:if>
  </xsl:template>
-->
  <!--
  <xsl:function name="mng:replaceVars">
    <xsl:param name="origVarNames" />
    <xsl:param name="newVarNames" />
    <xsl:param name="code" />
    <xsl:variable name="varSequence">
      <xsl:for-each select="$origVarNames">

        <xsl:sort select="string-length(.)" order="descending" />
        <xsl:element name="name">
          <xsl:value-of select="." />
        </xsl:element>
      </xsl:for-each>
    </xsl:variable>
    <xsl:variable name="tokenizedCode">
      <xsl:variable name="varRegEx" select="concat('(', string-join($varSequence/name, '|'), ')')" />
      <xsl:analyze-string select="$code" regex="{$varRegEx}">
        <xsl:matching-substring>
          <xsl:element name="codePart">
            <xsl:attribute name="type" select="'potentialVar'" />
            <xsl:value-of select="regex-group(1)" />
          </xsl:element>
        </xsl:matching-substring>
        <xsl:non-matching-substring>
          <xsl:element name="codePart">
            <xsl:attribute name="type" select="'code'" />
            <xsl:value-of select="." />
          </xsl:element>
        </xsl:non-matching-substring>
      </xsl:analyze-string>
    </xsl:variable>
    <xsl:for-each select="$tokenizedCode/codePart">
      <xsl:if test="@type eq 'potentialVar'">
        <xsl:variable name="potVarName" select="." />
        <xsl:if test="position() eq 1">
          <xsl:if test="matches(following-sibling::codePart[1], '^[^A-Za-z0-9_]')">
            <xsl:value-of select="concat('_l.', $newVarNames[count($origVarNames[every $n in 1 to position() - 1 satisfies $origVarNames[$n] ne $potVarName])])" />
          </xsl:if>
          <xsl:if test="not(matches(following-sibling::codePart[1], '^[^A-Za-z0-9_]'))">
            <xsl:value-of select="." />
          </xsl:if>
        </xsl:if>
        <xsl:if test="position() eq last()">
          <xsl:if test="matches(preceding-sibling::codePart[1], '[^A-Za-z_\.]$')">
            <xsl:value-of select="concat('_l.', $newVarNames[count($origVarNames[every $n in 1 to position() - 1 satisfies $origVarNames[$n] ne $potVarName])])" />
          </xsl:if>
          <xsl:if test="not(matches(preceding-sibling::codePart[1], '[^A-Za-z_\.]$'))">
            <xsl:value-of select="." />
          </xsl:if>
        </xsl:if>
        <xsl:if test="(position() ne 1) and (position() ne last())">
          <xsl:if test="(matches(preceding-sibling::codePart[1], '[^A-Za-z_\.]$')) and (matches(following-sibling::codePart[1], '^[^A-Za-z0-9_]'))">
            <xsl:value-of select="concat('_l.', $newVarNames[count($origVarNames[every $n in 1 to position() - 1 satisfies $origVarNames[$n] ne $potVarName])])" />
          </xsl:if>
          <xsl:if test="not((matches(preceding-sibling::codePart[1], '[^A-Za-z_\.]$')) and (matches(following-sibling::codePart[1], '^[^A-Za-z0-9_]')))">
            <xsl:value-of select="." />
          </xsl:if>
        </xsl:if>
      </xsl:if>
      <xsl:if test="@type eq 'code'">
        <xsl:value-of select="." />
      </xsl:if>
    </xsl:for-each>
  </xsl:function>
-->
  <xsl:function name="mng:replaceVars">
    <xsl:param name="origVarNames" />
    <xsl:param name="newVarNames" />
    <xsl:param name="code" />
    <xsl:variable name="varSequence">
      <xsl:for-each select="$origVarNames">

        <xsl:sort select="string-length(.)" order="descending" />
        <xsl:element name="name">
          <xsl:value-of select="." />
        </xsl:element>
      </xsl:for-each>
    </xsl:variable>
    <xsl:variable name="tokenizedCode">
      <xsl:variable name="varRegEx" select="concat('((([^A-Za-z0-9]|^|)(', string-join($varSequence/name, '|'), ')([^A-Za-z0-9_]|$))|([^\\]?&#x22;))')" />
      <xsl:analyze-string select="$code" regex="{$varRegEx}">
        <xsl:matching-substring>
          <xsl:if test="string-length(regex-group(2)) gt 0">
            <xsl:if test="string-length(regex-group(3)) gt 0">
              <xsl:element name="codePart">
                <xsl:attribute name="type" select="'code'" />
                <xsl:value-of select="regex-group(3)" />
              </xsl:element>
            </xsl:if>
            <!--    <xsl:if test="string-length(regex-group(3)) eq 0">
                <xsl:attribute name="type" select="'empty'" />
              </xsl:if> -->
            <xsl:element name="codePart">
              <xsl:attribute name="type" select="'potentialVar'" />
              <xsl:value-of select="regex-group(4)" />
            </xsl:element>
            <xsl:element name="codePart">
              <xsl:if test="string-length(regex-group(5)) gt 0">
                <xsl:attribute name="type" select="'code'" />
              </xsl:if>
              <xsl:if test="string-length(regex-group(5)) eq 0">
                <xsl:attribute name="type" select="'empty'" />
              </xsl:if>
              <xsl:value-of select="regex-group(5)" />
            </xsl:element>
          </xsl:if>
          <xsl:if test="string-length(regex-group(6)) gt 0">
            <xsl:if test="string-length(regex-group(6)) gt 1">
              <xsl:element name="codePart">
                <xsl:attribute name="type" select="'code'" />
                <xsl:value-of select="substring(regex-group(6), 1, 1)" />
              </xsl:element>
            </xsl:if>
            <xsl:element name="codePart">
              <xsl:attribute name="type" select="'quote'" />
              <xsl:value-of select="'&#x22;'" />
            </xsl:element>
          </xsl:if>
        </xsl:matching-substring>
        <xsl:non-matching-substring>
          <xsl:element name="codePart">
            <xsl:attribute name="type" select="'code'" />
            <xsl:value-of select="." />
          </xsl:element>
        </xsl:non-matching-substring>
      </xsl:analyze-string>
    </xsl:variable>
    <xsl:for-each select="$tokenizedCode/codePart">
      <xsl:if test="@type eq 'potentialVar'">
        <xsl:variable name="potVarName" select="." />
        <xsl:if test="position() eq 1">
          <xsl:if test="matches(substring(following-sibling::codePart[1], 1, 1), '^[^A-Za-z0-9_]')">
            <xsl:value-of select="concat('_l.', $newVarNames[index-of($origVarNames, $potVarName)])" />
          </xsl:if>
          <xsl:if test="not(matches(substring(following-sibling::codePart[1], 1, 1), '^[^A-Za-z0-9_]'))">
            <xsl:value-of select="." />
          </xsl:if>
        </xsl:if>
        <xsl:if test="position() eq last()">
          <xsl:if test="matches(substring(preceding-sibling::codePart[1], string-length(preceding-sibling::codePart[1]) - 1), '[^A-Za-z_\.]$')">
            <xsl:value-of select="concat('_l.', $newVarNames[index-of($origVarNames, $potVarName)])" />
          </xsl:if>
          <xsl:if test="not(matches(substring(preceding-sibling::codePart[1], string-length(preceding-sibling::codePart[1]) - 1), '[^A-Za-z_\.]$'))">
            <xsl:value-of select="." />
          </xsl:if>
        </xsl:if>
        <xsl:if test="(position() ne 1) and (position() ne last())">
          <xsl:if test="(count(preceding-sibling::codePart[@type eq 'quote']) mod 2) eq 0">
            <xsl:if test="(matches(substring(preceding-sibling::codePart[1], string-length(preceding-sibling::codePart[1])), '[^A-Za-z_\.]$')) or (preceding-sibling::codePart[1]/@type eq 'empty')">
              <xsl:if test="(matches(substring(following-sibling::codePart[1], 1, 1), '^[^A-Za-z0-9_]')) or (following-sibling::codePart[1]/@type eq 'empty')">
                <xsl:value-of select="concat('_l.', $newVarNames[index-of($origVarNames, $potVarName)])" />
              </xsl:if>
              <xsl:if test="not((matches(substring(following-sibling::codePart[1], 1, 1), '^[^A-Za-z0-9_]')) or (following-sibling::codePart[1]/@type eq 'empty'))">
                <xsl:value-of select="." />
              </xsl:if>
            </xsl:if>
            <xsl:if test="not((matches(substring(preceding-sibling::codePart[1], string-length(preceding-sibling::codePart[1])), '[^A-Za-z_\.]$')) or (preceding-sibling::codePart[1]/@type eq 'empty'))">
              <xsl:value-of select="." />
            </xsl:if>
          </xsl:if>
          <xsl:if test="(count(preceding-sibling::codePart[@type eq 'quote']) mod 2) eq 1">
            <xsl:value-of select="." />
          </xsl:if>
        </xsl:if>
      </xsl:if>
      <xsl:if test="@type eq 'quote'">
        <xsl:value-of select="." />
      </xsl:if>
      <xsl:if test="@type eq 'code'">
        <xsl:value-of select="." />
      </xsl:if>
    </xsl:for-each>
  </xsl:function>

  <xsl:template name="ConstructFunction">
    <xsl:param name="functionName"/>
    <xsl:param name="type"/>
    <xsl:param name="functionCode"/>
    <xsl:param name="params"/>
    <xsl:variable name="processedCode">
      <xsl:call-template name="mungeFunction">
        <xsl:with-param name="functionCode" select="$functionCode"/>
        <xsl:with-param name="params" select="$params"/>
      </xsl:call-template>
    </xsl:variable>
    <xsl:variable name="lineDelims">
      <xsl:element name="TermExpression">
        <xsl:attribute name="type" select="'Array'"/>
        <xsl:attribute name="openCount" select="xs:integer(0)"/>
        <xsl:text>\}\s*;\s*$</xsl:text>
      </xsl:element>
      <xsl:element name="TermExpression">
        <xsl:attribute name="type" select="'Return'"/>
        <xsl:attribute name="openCount" select="xs:integer(0)"/>
        <xsl:text>\s+return\s+?.+?;$</xsl:text>
      </xsl:element>
      <xsl:element name="TermExpression">
        <xsl:attribute name="type" select="'Return'"/>
        <xsl:attribute name="openCount" select="xs:integer(0)"/>
        <xsl:text>^return\s+?</xsl:text>
      </xsl:element>
      <xsl:element name="TermExpression">
        <xsl:attribute name="type" select="'OpenBrace'"/>
        <xsl:attribute name="openCount" select="xs:integer(1)"/>
        <xsl:text>\{\s*?$</xsl:text>
      </xsl:element>
      <xsl:element name="TermExpression">
        <xsl:attribute name="type" select="'TerminatingParen'"/>
        <xsl:attribute name="openCount" select="xs:integer(0)"/>
        <xsl:text>\)\s*$</xsl:text>
      </xsl:element>
      <xsl:element name="TermExpression">
        <xsl:attribute name="type" select="'Semi'"/>
        <xsl:attribute name="openCount" select="xs:integer(0)"/>
        <xsl:text>\}.*;\s*$</xsl:text>
      </xsl:element>
      <xsl:element name="TermExpression">
        <xsl:attribute name="type" select="'CloseBrace'"/>
        <xsl:attribute name="openCount" select="xs:integer(-1)"/>
        <xsl:text>\}</xsl:text>
      </xsl:element>
      <xsl:element name="TermExpression">
        <xsl:attribute name="type" select="'Else'"/>
        <xsl:attribute name="openCount" select="xs:integer(0)"/>
        <xsl:text>[^0-9a-zA-Z_]*?else[^0-9a-zA-Z_]*?</xsl:text>
      </xsl:element>
      <xsl:element name="TermExpression">
        <xsl:attribute name="type" select="'Semi'"/>
        <xsl:attribute name="openCount" select="xs:integer(0)"/>
        <xsl:text>;\s*$</xsl:text>
      </xsl:element>
    </xsl:variable>
    <xsl:variable name="delimitedCode">
      <xsl:variable name="delimRegEx"
                    select="concat('(', string-join($lineDelims/TermExpression, '|'), ')')"/>
      <xsl:for-each select="$processedCode/Code">
        <xsl:analyze-string select="." regex="{$delimRegEx}">
          <xsl:matching-substring>
            <xsl:element name="CodeDelim">
              <xsl:variable name="term" select="$lineDelims/TermExpression[matches(regex-group(1), .)][1]"/>
              <xsl:attribute name="DelimType" select="$term/@type"/>
              <xsl:attribute name="OpenCount" select="$term/@openCount"/>
              <xsl:value-of select="regex-group(1)"/>
            </xsl:element>
          </xsl:matching-substring>
          <xsl:non-matching-substring>
            <xsl:element name="CodePart">
              <xsl:value-of select="normalize-space(.)"/>
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
                <xsl:value-of select="count(preceding-sibling::CodeDelim[. eq 'OpenBrace'])"/>
              </xsl:element>
              <xsl:element name="CloseNdx">
                <xsl:value-of select="position()"/>
              </xsl:element>
            </xsl:element>
          </xsl:if>
        </xsl:for-each>
      </xsl:variable>
      <xsl:for-each select="$delimitedCode/CodeDelim">
        <xsl:variable name="ndx" select="position()"/>
        <xsl:variable name="code">
          <xsl:value-of select="preceding-sibling::CodePart[1]"/>
        </xsl:variable>
        <xsl:if test="name() eq 'CodeDelim'">
          <xsl:variable name="delim" select="."/>
          <xsl:element name="Code">
            <xsl:choose>
              <xsl:when test="count(preceding-sibling::CodeDelim) eq 0">
                <xsl:attribute name="Depth" select="xs:integer(0)"/>
              </xsl:when>
              <xsl:when test="matches(@DelimType, 'Array')">
                <xsl:attribute name="Depth" select="sum(preceding-sibling::CodeDelim/@OpenCount)"/>
              </xsl:when>
              <xsl:when test="matches(@DelimType, '(CloseBrace|BraceElse|BraceElseBrace)')">
                <xsl:attribute name="Depth" select="sum(preceding-sibling::CodeDelim/@OpenCount) - 1"/>
              </xsl:when>
              <xsl:when test="matches(@DelimType, '(OpenBrace|ElseBrace)')">
                <xsl:attribute name="Depth" select="sum(preceding-sibling::CodeDelim/@OpenCount)"/>
              </xsl:when>
              <xsl:when test="matches(@DelimType, '(Else|TerminatingParen)')">
                <xsl:attribute name="Depth" select="sum(preceding-sibling::CodeDelim/@OpenCount)"/>
              </xsl:when>
              <xsl:otherwise>
                <xsl:if test="count(preceding-sibling::CodeDelim) gt 0">
                  <xsl:attribute name="Depth" select="sum(preceding-sibling::CodeDelim/@OpenCount)"/>
                </xsl:if>
                <xsl:if test="count(preceding-sibling::CodeDelim) eq 0">
                  <xsl:attribute name="Depth" select="xs:integer(0)"/>
                </xsl:if>
              </xsl:otherwise>
            </xsl:choose>
            <xsl:attribute name="Position" select="position()"/>
            <xsl:choose>
              <xsl:when test="$delim/@DelimType eq 'Semi'">
                <xsl:if test="(count(preceding-sibling::CodeDelim) eq 0) or (preceding-sibling::CodeDelim[1]/@DelimType ne 'Return')">
                  <xsl:attribute name="BlockID" select="'-1'"/>
                  <xsl:attribute name="CodeType" select="'Line'"/>
                  <xsl:if test="string-length($code) gt 0">
                    <xsl:value-of select="concat($code, normalize-space($delim))"/>
                  </xsl:if>
                </xsl:if>
              </xsl:when>
              <xsl:when test="$delim/@DelimType eq 'Return'">
                <xsl:attribute name="BlockID" select="'-1'"/>
                <xsl:attribute name="CodeType" select="'Return'"/>
                <xsl:attribute name="ReturnedVal" select="following-sibling::CodePart[1]"/>
              </xsl:when>
              <xsl:when test="$delim/@DelimType eq 'Else'">
                <xsl:attribute name="BlockID" select="'-1'"/>
                <xsl:attribute name="ParentType" select="'Else'"/>
                <xsl:attribute name="CodeType" select="'Parent'"/>
              </xsl:when>
              <xsl:when test="($delim/@DelimType eq 'TerminatingParen') and (following-sibling::*[1]/name() ne 'CodeDelim')">
                <xsl:attribute name="BlockID" select="'-1'"/>
                <xsl:attribute name="CodeType" select="'Parent'"/>
                <xsl:choose>
                  <xsl:when test="starts-with(lower-case($code), 'for')">
                    <xsl:attribute name="ParentType" select="'for'"/>
                    <xsl:variable name="var"
                                  select="replace(substring-after($code, 'for'), '^\s*\((var\s+)?([_A-Za-z][0-9A-Za-z_\.]*).*?$', '$2')"/>
                    <xsl:attribute name="Var" select="$var"/>
                    <xsl:attribute name="StartValue"
                                   select="replace(concat(normalize-space(substring-after($code, '=')), ')'), '^(.+?)(.*)$', '$1')"/>
                    <xsl:variable name="comparison"
                                  select="replace(normalize-space(substring-after($code, ';')), '^(.+?)(;.*)$', '$1')"/>
                    <xsl:attribute name="Comparison" select="$comparison"/>
                    <xsl:attribute name="VarChange"
                                   select="normalize-space(replace(normalize-space(substring-after($code, $comparison)), ';(.+?)$', '$1'))"/>
                  </xsl:when>
                  <xsl:when test="starts-with(lower-case(normalize-space($code)), 'while')">
                    <xsl:attribute name="ParentType" select="'while'"/>
                    <xsl:attribute name="Condition" select="replace($code, '(.*?while.*?)\((.*)\)*$', '$2')"/>
                  </xsl:when>
                  <xsl:when test="starts-with(lower-case(normalize-space($code)), 'if')">
                    <xsl:attribute name="ParentType" select="'if'"/>
                    <xsl:attribute name="Condition"
                                   select="replace(normalize-space(substring-after($code, '(')), '^(.+?)$', '($1)')"/>
                  </xsl:when>
                </xsl:choose>
              </xsl:when>
              <xsl:when test="$delim/@DelimType eq 'OpenBrace'">
                <xsl:attribute name="BlockID"
                               select="count(preceding-sibling::CodeDelim[. eq 'OpenBrace']) + 1"/>
                <xsl:attribute name="CodeType" select="'OpenBlock'"/>
                <xsl:choose>
                  <xsl:when test="starts-with(lower-case($code), 'for')">
                    <xsl:attribute name="BlockType" select="'for'"/>
                    <xsl:variable name="var"
                                  select="replace(substring-after($code, 'for'), '^\s*\((var\s+)?([_A-Za-z][0-9A-Za-z_\.]*).*?$', '$2')"/>
                    <xsl:attribute name="Var" select="$var"/>
                    <xsl:attribute name="StartValue"
                                   select="replace(concat(normalize-space(substring-after($code, '=')), ')'), '^(.+?)(;.*)$', '$1')"/>
                    <xsl:variable name="comparison"
                                  select="replace(normalize-space(substring-after($code, ';')), '^(.+?)(;.*)$', '$1')"/>
                    <xsl:attribute name="Comparison" select="$comparison"/>
                    <xsl:attribute name="VarChange"
                                   select="normalize-space(replace(concat(normalize-space(substring-before(substring-after($code, $comparison), ')')), ')'), ';(.+?)\)$', '$1'))"/>
                  </xsl:when>
                  <xsl:when test="starts-with(lower-case(normalize-space($code)), 'while')">
                    <xsl:attribute name="BlockType" select="'while'"/>
                    <xsl:attribute name="Condition" select="replace($code, '(.*?while.*?)\((.*?)\)', '$2')"/>
                  </xsl:when>
                  <xsl:when test="starts-with(lower-case($code), 'do')">
                    <xsl:attribute name="BlockType" select="'do'"/>
                  </xsl:when>
                  <xsl:when test="starts-with(lower-case(normalize-space($code)), 'if')">
                    <xsl:attribute name="BlockType" select="'if'"/>
                    <xsl:attribute name="Condition"
                                   select="replace(normalize-space(substring-after($code, '(')), '^(.+?)\)$', '($1)')"/>
                  </xsl:when>
                  <xsl:otherwise>
                    <xsl:attribute name="BlockType" select="'none'"/>
                  </xsl:otherwise>
                </xsl:choose>
              </xsl:when>
              <xsl:when test="$delim/@DelimType eq 'Array'">
                <xsl:variable name="pos" select="xs:integer(position())"/>
                <xsl:attribute name="CodeType" select="'Array'"/>
                <xsl:attribute name="ArrayCode" select="preceding-sibling::CodePart[1]"/>
              </xsl:when>
              <xsl:when test="$delim/@DelimType eq 'CloseBrace'">
                <xsl:variable name="pos" select="xs:integer(position())"/>
                <xsl:attribute name="BlockID" select="$blockIDTable[xs:integer(CloseNdx) eq $pos]/BlockID"/>
                <xsl:attribute name="CodeType" select="'CloseBlock'"/>
                <xsl:choose>
                  <xsl:when test="following-sibling::*[1]/name() eq 'CodePart'">
                    <xsl:variable name="followingCode" select="normalize-space(following-sibling::CodePart[1])"/>
                    <xsl:if test="starts-with($followingCode, 'while')">
                      <xsl:attribute name="BlockTermType" select="'DoWhile'"/>
                      <xsl:variable name="whileClause"
                                    select="normalize-space(substring-after($followingCode, 'while'))"/>
                      <xsl:attribute name="Condition"
                                     select="substring($whileClause, 2, string-length($followingCode) - 2)"/>
                    </xsl:if>
                    <xsl:if test="not(starts-with($followingCode, 'while'))">
                      <xsl:attribute name="BlockTermType" select="'Term'"/>
                    </xsl:if>
                  </xsl:when>
                  <xsl:otherwise>
                    <xsl:attribute name="BlockTermType" select="'Term'"/>
                  </xsl:otherwise>
                </xsl:choose>
              </xsl:when>
            </xsl:choose>
          </xsl:element>
        </xsl:if>
      </xsl:for-each>
    </xsl:variable>
    <xsl:variable name="sequences">
      <xsl:if test="count($functionXML/Code) eq 0">
        <xsl:element name="CodeSequence">
          <xsl:variable name="startPos" select="0"/>
          <xsl:attribute name="Depth" select="0"/>
          <xsl:attribute name="numFollowing" select="0"/>
          <xsl:attribute name="Length" select="0"/>
          <xsl:attribute name="SequenceNum" select="1"/>
          <xsl:copy-of select="$functionXML/Code"/>
        </xsl:element>
      </xsl:if>
      <xsl:if test="count($functionXML/Code) eq 1">
        <xsl:element name="CodeSequence">
          <xsl:variable name="startPos" select="1"/>
          <xsl:attribute name="Depth" select="0"/>
          <xsl:attribute name="numFollowing" select="0"/>
          <xsl:attribute name="Length" select="1"/>
          <xsl:attribute name="SequenceNum" select="1"/>
          <xsl:copy-of select="$functionXML/Code"/>
        </xsl:element>
      </xsl:if>
      <xsl:if test="count($functionXML/Code) gt 1">
        <xsl:for-each select="$functionXML/Code[(position() eq 1) or (@Depth ne preceding-sibling::Code[1]/@Depth)]">
          <xsl:variable name="segDepth" select="@Depth"/>
          <xsl:variable name="codeNode" select="."/>
          <xsl:variable name="length">
            <xsl:if test="position() eq last()">
              <xsl:value-of select="count(following-sibling::Code) + 1"/>
            </xsl:if>
            <xsl:if test="position() ne last()">
              <xsl:value-of select="count(following-sibling::Code[@Depth eq $segDepth][every $p in preceding-sibling::Code intersect $codeNode/following-sibling::Code satisfies $p/@Depth eq $segDepth]) + 1"/>
            </xsl:if>
          </xsl:variable>
          <xsl:element name="CodeSequence">
            <xsl:variable name="startPos" select="xs:integer(@Position)"/>
            <xsl:attribute name="Depth" select="$segDepth"/>
            <xsl:attribute name="numFollowing" select="count(following-sibling::Code)"/>
            <xsl:attribute name="Length" select="$length"/>
            <xsl:attribute name="SequenceNum" select="position()"/>
            <xsl:copy-of select="(., following-sibling::Code[position() lt xs:integer($length)])"/>
          </xsl:element>
        </xsl:for-each>
      </xsl:if>
    </xsl:variable>
    <xsl:variable name="formattedSequences">
      <xsl:for-each select="$sequences/CodeSequence">
        <xsl:variable name="thisSequence" select="."/>
        <xsl:element name="CodeSequence">
          <xsl:attribute name="Depth" select="@Depth"/>
          <xsl:attribute name="Position" select="position()"/>
          <xsl:attribute name="OpenRole" select="Code[1]/@CodeType"/>
          <xsl:attribute name="CloseRole" select="Code[last()]/@CodeType"/>
          <xsl:attribute name="ContainsReturn"
                         select="if (some $c in Code satisfies $c/@CodeType eq 'Return') then 'yes' else 'no'"/>
          <xsl:for-each select="$thisSequence/Code">
            <xsl:if test="@CodeType eq 'Line'">
              <xsl:element name="Line">
                <xsl:if test="string-length(.) gt 0">
                  <xsl:value-of select="."/>
                </xsl:if>
                <xsl:if test="string-length(.) eq 0">
                  <xsl:value-of select="';'"/>
                </xsl:if>
              </xsl:element>
            </xsl:if>
            <xsl:if test="@CodeType ne 'Line'">
              <xsl:call-template name="OutputNonLine">
                <xsl:with-param name="elem" select="."/>
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
            <xsl:value-of select="."/>
          </xsl:element>
          <xsl:element name="NewParam">
            <xsl:value-of select="concat('_p', position())"/>
          </xsl:element>
        </xsl:element>
      </xsl:for-each>
    </xsl:variable>
    <xsl:variable name="depths">
      <xsl:for-each select="$formattedSequences/CodeSequence">
        <xsl:element name="Depth">
          <xsl:value-of select="xs:integer(@Depth)"/>
        </xsl:element>
      </xsl:for-each>
    </xsl:variable>
    <xsl:variable name="maxDepth" select="max($depths/Depth)"/>
    <xsl:if test="xs:integer($maxDepth) eq 0">
      <xsl:copy-of select="mng:ConstructSubFunction($formattedSequences, $type, $ParamTable/ParamEntry/NewParam, $functionName, 0, 0)" />
    </xsl:if>
    <xsl:if test="xs:integer($maxDepth) gt 0">
      <xsl:copy-of select="mng:ConstructSubFunction($formattedSequences, $type, $ParamTable/ParamEntry/NewParam, $functionName, 0, 0)" />
      <xsl:for-each select="1 to xs:integer($maxDepth)">
        <xsl:variable name="depth" select="."/>
        <xsl:variable name="thisDepthSequences">
          <xsl:for-each select="$formattedSequences/CodeSequence">
            <xsl:variable name="codeSeq" select="."/>
            <xsl:variable name="codePos" select="position()"/>
            <xsl:element name="seqElem">
              <xsl:element name="newSeqNdx">
                <xsl:value-of select="if (xs:integer($codeSeq/@Depth) ge $depth) then xs:integer($codePos) else -1"/>
              </xsl:element>
              <xsl:element name="depth">
                <xsl:value-of select="@Depth" />
              </xsl:element>
            </xsl:element>
          </xsl:for-each>
        </xsl:variable>
        <xsl:for-each select="$thisDepthSequences/seqElem[xs:integer(preceding-sibling::seqElem[1]/newSeqNdx) eq -1][xs:integer(newSeqNdx) ne -1]">
          <xsl:variable name="startCodePos" select="xs:integer(newSeqNdx)"/>
          <xsl:variable name="endCodePos">
            <xsl:if test="position() eq last()">
              <xsl:value-of select="max($thisDepthSequences/seqElem/newSeqNdx)"/>
            </xsl:if>
            <xsl:if test="position() lt last()">
              <xsl:value-of select="$thisDepthSequences/seqElem[position() ge $startCodePos][(xs:integer(newSeqNdx) ne -1) and (xs:integer(following-sibling::seqElem[1]/newSeqNdx) eq -1)][1]/newSeqNdx"/>
            </xsl:if>
          </xsl:variable>
          <xsl:variable name="subSeqs">
            <xsl:for-each select="xs:integer($startCodePos) to xs:integer($endCodePos)">
              <xsl:variable name="seqPos" select="xs:integer(.)"/>
              <xsl:copy-of select="$formattedSequences/CodeSequence[$seqPos]"/>
            </xsl:for-each>
          </xsl:variable>
          <xsl:variable name="precedingDepthValues" select="$thisDepthSequences/seqElem[position() gt 1 and position() lt $startCodePos][xs:integer(newSeqNdx) ge 0]" />
          <xsl:variable name="numPrecedingGreaterDepthSeqs" select="$precedingDepthValues[xs:integer(preceding-sibling::seqElem[1]/depth) eq xs:integer($depth)][every $i in xs:integer(following-sibling::seqElem[1]/newSeqNdx) satisfies $i ne -1 ][xs:integer(depth) eq xs:integer($depth) + 1]" />
          <xsl:copy-of select="mng:ConstructSubFunction($subSeqs, $type, (), $functionName,  position(), count($numPrecedingGreaterDepthSeqs))" />
        </xsl:for-each>
      </xsl:for-each>
    </xsl:if>
  </xsl:template>

  <xsl:function name="mng:ConstructSubFunction">
    <xsl:param name="functionSegments"/>
    <xsl:param name="type"/>
    <xsl:param name="params"/>
    <xsl:param name="functName"/>
    <xsl:param name="segNum"/>
    <xsl:param name="subFunctStartNdx"/>
    <xsl:variable name="parentDepth"
                  select="min(for $i in 1 to count($functionSegments/CodeSequence) return $functionSegments/CodeSequence[$i]/@Depth)"/>
    <xsl:variable name="depths">
      <xsl:for-each select="$functionSegments/CodeSequence">
        <xsl:element name="Depth">
          <xsl:value-of select="xs:integer(@Depth)" />
        </xsl:element>
      </xsl:for-each>
    </xsl:variable>
    <xsl:variable name="segItrVals">
      <xsl:value-of select="(1 to count($functionSegments/CodeSequence))"/>
    </xsl:variable>
    <xsl:element name="SubFunction">
      <xsl:attribute name="Params" select="$params"/>
      <xsl:attribute name="FunctionName">
        <xsl:if test="$parentDepth eq 0">
          <xsl:value-of select="$functName"/>
        </xsl:if>
        <xsl:if test="$parentDepth gt 0">
          <xsl:value-of select="concat($functName, '.s', $parentDepth, '_', $segNum)"/>
        </xsl:if>
      </xsl:attribute>
      <xsl:attribute name="SubParam">
        <xsl:if test="$parentDepth eq 0">
          <xsl:value-of select="'_p'"/>
        </xsl:if>
        <xsl:if test="$parentDepth gt 0">
          <xsl:value-of select="'_l'"/>
        </xsl:if>
      </xsl:attribute>
      <xsl:variable name="initL">
        <xsl:if test="(some $cs in $functionSegments/CodeSequence[xs:integer(@Depth) gt 0] satisfies ($cs/@ContainsReturn eq 'yes'))">
          <xsl:element name="Line">
            <xsl:value-of select="'_l._hr = false;'"/>
          </xsl:element>
          <xsl:element name="Line">
            <xsl:value-of select="'_l._rv = null;'"/>
          </xsl:element>
        </xsl:if>
        <xsl:if test="count($params) gt 0">
          <xsl:element name="Line">
            <xsl:value-of select="'_l._p = _p;'"/>
          </xsl:element>
        </xsl:if>
      </xsl:variable>
      <xsl:for-each select="1 to count($depths/Depth)">
        <xsl:variable name="ndx" select="position()"/>
        <xsl:if test="position() eq 1">
          <xsl:if test="$parentDepth eq 0">
            <xsl:element name="Line">
              <xsl:value-of select="'var _l = new Object();'"/>
            </xsl:element>
            <xsl:copy-of select="$initL/Line"/>
          </xsl:if>
        </xsl:if>
        <xsl:if test="xs:integer($depths/Depth[$ndx eq position()]) eq $parentDepth">
          <xsl:if test="count($functionSegments/CodeSequence) eq 1">
            <xsl:copy-of select="$functionSegments/CodeSequence/Line" />
          </xsl:if>
          <xsl:if test="count($functionSegments/CodeSequence) gt 1">
            <xsl:copy-of select="$functionSegments/CodeSequence[$ndx]/Line" />
          </xsl:if>
        </xsl:if>
        <xsl:if test="(xs:integer($depths/Depth[$ndx eq position()]) eq ($parentDepth + 1)) and (xs:integer($depths/Depth[$ndx - 1]) eq $parentDepth)">
          <xsl:element name="Line">
            <xsl:variable name="subDepth" select="$parentDepth + 1"/>
            <xsl:variable name="subFunctName"
                          select="concat($functName, '.s', $subDepth, '_', xs:integer($subFunctStartNdx) + count($depths/Depth[position() lt $ndx][xs:integer(.) eq xs:integer(preceding-sibling::Depth[1]) + 1][xs:integer(.) eq $parentDepth + 1]) + 1)"/>
            <xsl:if test="not(matches($type, 'function'))">
              <xsl:value-of select="concat($subFunctName, '.cEval(this, _l);')"/>
            </xsl:if>
            <xsl:if test="matches($type, 'function')">
              <xsl:value-of select="concat($subFunctName, '.fEval(_l);')"/>
            </xsl:if>
          </xsl:element>
          <xsl:if test="(some $cs in $functionSegments/CodeSequence[xs:integer(@Depth) gt 0] satisfies ($cs/@ContainsReturn eq 'yes'))">
            <xsl:element name="Line">
              <xsl:value-of select="'if (_l._hr == true) return _l._rv;'"/>
            </xsl:element>
          </xsl:if>
        </xsl:if>
      </xsl:for-each>
    </xsl:element>
  </xsl:function>

  <xsl:template name="OutputNonLine">
    <xsl:param name="elem"/>
    <xsl:variable name="elemType">
      <xsl:choose>
        <xsl:when test="$elem/@CodeType eq 'Parent'">
          <xsl:value-of select="$elem/@ParentType"/>
        </xsl:when>
        <xsl:when test="$elem/@CodeType eq 'OpenBlock'">
          <xsl:value-of select="$elem/@BlockType"/>
        </xsl:when>
        <xsl:when test="$elem/@CodeType eq 'CloseBlock'">
          <xsl:value-of select="$elem/@BlockTermType"/>
        </xsl:when>
        <xsl:when test="$elem/@CodeType eq 'Return'">
          <xsl:value-of select="'Return'"/>
        </xsl:when>
        <xsl:when test="$elem/@CodeType eq 'Array'">
          <xsl:value-of select="$elem/@CodeType"/>
        </xsl:when>
      </xsl:choose>
    </xsl:variable>
    <xsl:choose>
      <xsl:when test="$elemType eq 'Else'">
        <xsl:element name="Line">
          <xsl:value-of select="'else'"/>
        </xsl:element>
      </xsl:when>
      <xsl:when test="$elemType eq 'for'">
        <xsl:element name="Line">
          <xsl:value-of select="concat('for (', (if (@VarDeclared) then 'var' else ''), @Var, ' = ', @StartValue, '; ', @Comparison, '; ', @VarChange, ')', (if ($elem/@CodeType eq 'OpenBlock') then ' {' else ''))"/>
        </xsl:element>
      </xsl:when>
      <xsl:when test="$elemType eq 'while'">
        <xsl:element name="Line">
          <xsl:value-of select="concat('while (', @Condition, (if (@CodeType eq 'OpenBlock') then ') {' else ')'))"/>
        </xsl:element>
      </xsl:when>
      <xsl:when test="$elemType eq 'do'">
        <xsl:element name="Line">
          <xsl:value-of select="'do {'"/>
        </xsl:element>
      </xsl:when>
      <xsl:when test="$elemType eq 'Array'">
        <xsl:element name="Line">
          <xsl:value-of select="concat($elem/@ArrayCode, '};')"/>
        </xsl:element>
      </xsl:when>
      <xsl:when test="$elemType eq 'DoWhile'">
        <xsl:element name="Line">
          <xsl:value-of select="concat('while ', @Condition, ';')"/>
        </xsl:element>
      </xsl:when>
      <xsl:when test="$elemType eq 'Term'">
        <xsl:element name="Line">
          <xsl:value-of select="'}'"/>
        </xsl:element>
      </xsl:when>
      <xsl:when test="$elemType eq 'if'">
        <xsl:element name="Line">
          <xsl:value-of select="concat('if ', @Condition, if ($elem/@CodeType eq 'OpenBlock') then '{' else '')"/>
        </xsl:element>
      </xsl:when>
      <xsl:when test="$elemType eq 'none'">
        <xsl:if test="$elem/@CodeType eq 'OpenBlock'">
          <xsl:element name="Line">
            <xsl:value-of select="'{'"/>
          </xsl:element>
        </xsl:if>
      </xsl:when>
      <xsl:when test="$elemType eq 'Return'">
        <xsl:element name="Line">
          <xsl:value-of select="'_l._hr = true;'"/>
        </xsl:element>
        <xsl:element name="Line">
          <xsl:value-of select="concat('_l._rv = ', $elem/@ReturnedVal, ';')"/>
        </xsl:element>
        <xsl:element name="Line">
          <xsl:value-of select="'return _l._rv;'"/>
        </xsl:element>
      </xsl:when>
    </xsl:choose>
  </xsl:template>

  <xsl:template name="buildFunctDescriptor">
    <xsl:param name="entityType" />
    <xsl:param name="source" />
    <xsl:param name="functions" />
    <xsl:param name="entityNdx" />
    <xsl:variable name="classPrefix">
      <xsl:if test="$entityType eq 'class'">
        <xsl:value-of select="$source/@ClassPrefix" />
      </xsl:if>
      <xsl:if test="$entityType ne 'class'">
        <xsl:value-of select="''" />
      </xsl:if>
    </xsl:variable>
    <xsl:variable name="classFunctionPrefix">
      <xsl:if test="$entityType eq 'class'">
        <xsl:value-of select="$source/@ClassFunctionPrefix" />
      </xsl:if>
      <xsl:if test="$entityType ne 'class'">
        <xsl:value-of select="''" />
      </xsl:if>
    </xsl:variable>
    <xsl:variable name="functionPrefix">
      <xsl:if test="$entityType eq 'function'">
        <xsl:value-of select="$source/@FunctionPrefix" />
      </xsl:if>
      <xsl:if test="$entityType ne 'function'">
        <xsl:value-of select="''" />
      </xsl:if>
    </xsl:variable>
    <xsl:variable name="descriptRegEx">
      <xsl:if test="$entityType eq 'class'" >
        <xsl:value-of select="concat('(', $classPrefix, $entityNdx, ')((\.(', $classFunctionPrefix, '[0-9]+))?)((\.s([0-9]+))?)((_([0-9]+))?)')" />
      </xsl:if>
      <xsl:if test="$entityType eq 'function'">
        <xsl:value-of select="concat('()(())(', $functionPrefix, $entityNdx, ')((\.s([0-9]+))?)((_([0-9]+))?)')" />
      </xsl:if>
    </xsl:variable>
    <xsl:variable name="functionList">
      <xsl:for-each select="$functions">
        <xsl:analyze-string select="." regex="{$descriptRegEx}" >
          <xsl:matching-substring>
            <xsl:element name="Function">
              <xsl:element name="ClassName">
                <xsl:if test="string-length(regex-group(1)) gt 0">
                  <xsl:value-of select="regex-group(1)" />
                </xsl:if>
                <xsl:if test="empty(regex-group(1))">
                  <xsl:value-of select="''" />
                </xsl:if>
              </xsl:element>
              <xsl:element name="FunctionName">
                <xsl:value-of select="regex-group(4)" />
              </xsl:element>
              <xsl:element name="Depth">
                <xsl:if test="string-length(regex-group(7)) eq 0">
                  <xsl:value-of select="'0'" />
                </xsl:if>
                <xsl:if test="string-length(regex-group(7)) gt 0">
                  <xsl:value-of select="regex-group(7)" />
                </xsl:if>
              </xsl:element>
              <xsl:element name="Segment">
                <xsl:if test="string-length(regex-group(10)) eq 0">
                  <xsl:value-of select="'0'" />
                </xsl:if>
                <xsl:if test="string-length(regex-group(10)) gt 0">
                  <xsl:value-of select="regex-group(10)" />
                </xsl:if>
              </xsl:element>
            </xsl:element>
          </xsl:matching-substring>
        </xsl:analyze-string>
      </xsl:for-each>
    </xsl:variable>
    <xsl:element name="Descriptor">

      <xsl:variable name="groupBy">
        <xsl:if test="every $cn in $functionList/Function/ClassName satisfies string-length($cn) eq 0">
          <xsl:value-of select="$functionPrefix" />
        </xsl:if>
        <xsl:if test="some $cn in $functionList/Function/ClassName satisfies string-length($cn) gt 0">
          <xsl:value-of select="$classFunctionPrefix" />
        </xsl:if>
      </xsl:variable>
      <xsl:element name="FunctionDescriptors">
        <xsl:for-each-group select="$functionList/Function" group-by="FunctionName" >
          <xsl:variable name="functs" select="current-group()" />
          <xsl:element name="FunctionDescriptor">
            <xsl:if test="$entityType eq 'class'">
              <xsl:attribute name="ClassName" select="ClassName" />
            </xsl:if>
            <xsl:if test="string-length(FunctionName) gt 0">
              <xsl:attribute name="FunctionName" select="FunctionName" />
            </xsl:if>
            <xsl:if test="$entityType eq 'class'">
              <xsl:attribute name="IsGlobalFunction" select="'no'" />
            </xsl:if>
            <xsl:if test="$entityType ne 'class'">
              <xsl:attribute name="IsGlobalFunction" select="'yes'" />
            </xsl:if>
            <xsl:element name="Segments">
              <xsl:for-each-group select="$functs" group-by="Depth">
                <xsl:element name="Segment">
                  <xsl:variable name="funct" select="." />
                  <xsl:if test="position() gt 1">
                    <xsl:value-of select="max(current-group()[string-length($funct/Segment) gt 0]/Segment)" />
                  </xsl:if>
                  <xsl:if test="position() eq 1">
                    <xsl:value-of select="'1'" />
                  </xsl:if>
                </xsl:element>
              </xsl:for-each-group>
            </xsl:element>
          </xsl:element>
        </xsl:for-each-group>
      </xsl:element>
    </xsl:element>
  </xsl:template>
</xsl:stylesheet>
