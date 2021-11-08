<?xml version="1.0" encoding="UTF-8"?>
<xsl:stylesheet xmlns:xs="http://www.w3.org/2001/XMLSchema"
                xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
                version="2.0"
                exclude-result-prefixes="xs">

  <xsl:output method="xml" encoding="utf-8" indent="yes"/>

  <xsl:template match="//SubFunctDescriptor">
    <xsl:element name="CodeFile">
      <xsl:element name="Class">
        <xsl:attribute name="ClassName" select="'_tc_'" />
        <xsl:attribute name="ClassPrefix" select="'_tc'" />
        <xsl:attribute name="ClassFunctionPrefix" select="'_tcf'" />
        <xsl:attribute name="ClassNdx" select="0" />
        <xsl:element name="Params">
          <xsl:element name="Param">functionList</xsl:element>
        </xsl:element>
        <xsl:element name="Constructor">
          <xsl:variable name="constructorCode">
            <xsl:text>
            this.durations = new Array();
            this.refreshed = false;
            this.startCount = 0;
            this.functList = functionList;
            this.pendingTimeouts = 0;
            if (duration) {
              this.durations.push(duration);
              this.startCount++;
              this.setTick(duration);
            }
            </xsl:text>
          </xsl:variable>
          <xsl:element name="ConstructorBody">
            <xsl:for-each select="tokenize($constructorCode, '&#x0A;')">
              <xsl:if test="string-length(normalize-space(.)) gt 0">
                <xsl:element name="Code">
                  <xsl:value-of select="normalize-space(.)"/>
                </xsl:element>
              </xsl:if>
            </xsl:for-each>
          </xsl:element>
        </xsl:element>
        <xsl:element name="PrototypeChain">
          <xsl:element name="Function">
            <xsl:attribute name="FunctionName" select="'start'" />
            <xsl:element name="Params">
              <xsl:element name="Param">duration</xsl:element>
            </xsl:element>
            <xsl:variable name="functionBodyElems">
              <xsl:text>
                setTimeout(this.tick(this.startCount), this.durations[this.durations.length - 1]);
              </xsl:text>
            </xsl:variable>
            <xsl:element name="FunctionBody">
              <xsl:for-each select="tokenize($functionBodyElems, '&#x0A;')">
                <xsl:if test="string-length(normalize-space(.)) gt 0">
                  <xsl:element name="Code">
                    <xsl:value-of select="normalize-space(.)"/>
                  </xsl:element>
                </xsl:if>
              </xsl:for-each>
            </xsl:element>
          </xsl:element>

          <xsl:element name="Function">
            <xsl:attribute name="FunctionName" select="'refresh'" />
            <xsl:element name="Params" />
            <xsl:element name="FunctionBody">
              <xsl:element name="Code">this.refreshed = true;</xsl:element>
            </xsl:element>
          </xsl:element>

          <xsl:element name="Function">
            <xsl:attribute name="FunctionName" select="'setTick'" />
            <xsl:element name="Params">
              <xsl:element name="Param">duration</xsl:element>
            </xsl:element>
            <xsl:element name="FunctionBody">
              <xsl:text>
                setTimeout(this.tick(this.startCount++), duration);
                this.pendingTimeouts++;
              </xsl:text>
            </xsl:element>
          </xsl:element>

          <xsl:element name="Function">
            <xsl:attribute name="FunctionName" select="'tick'" />
            <xsl:element name="Params">
              <xsl:element name="Param">instanceNum</xsl:element>
            </xsl:element>
            <xsl:variable name="functionBody">
              <xsl:text>
              this.pendingTimeouts--;
              var dur = this.durations[instanceNum - 1];
              var bFail = true;
              if (this.durations[instanceNum - 1] == 0) {
                this.durations[instanceNum - 1] = -1;
                this.startCount--;
                bFail = false;
              }
              if (this.pendingTimeouts == 0) {
                var ndx = 0;
                while (ndx &lt; this.durations.length) {
                  if (this.durations[ndx] == -1)
                    this.durations.splice(ndx, 1);
                  else
                    ndx++;
                }
              }
              else if (this.refreshed) {
                this.setTick(dur);
                this.refreshed = false;
              } else if (bFail)
                this.timerExpired();
            </xsl:text>
            </xsl:variable>
            <xsl:element name="FunctionBody">
              <xsl:for-each select="tokenize($functionBody, '&#x0A;')">
                <xsl:if test="string-length(normalize-space(.)) gt 0">
                  <xsl:element name="Code">
                    <xsl:value-of select="normalize-space(.)" />
                  </xsl:element>
                </xsl:if>
              </xsl:for-each>
            </xsl:element>
          </xsl:element>

          <xsl:element name="Function">
            <xsl:attribute name="FunctionName" select="'stop'" />
            <xsl:element name="Params" />
            <xsl:element name="FunctionBody">
              <xsl:element name="Code">this.durations[this.durations.length - 1] = 0;</xsl:element>
            </xsl:element>
          </xsl:element>

          <xsl:element name="Function">
            <xsl:attribute name="FunctionName" select="'timerExpired'" />
            <xsl:element name="Params" />
            <xsl:variable name="functionCode">
              <xsl:text>
              var elem;
              if (for var ctr = 0; ctr &lt; this.functList.length; ctr++) {
                elem = document.getElementByTagName(this.functList[ctr]);
                document.removeChild(elem);
              }
              window.location.assign("/ServerError?Error=12&amp;ServerResponse=-1");
            </xsl:text>
            </xsl:variable>
            <xsl:element name="FunctionBody">
              <xsl:for-each select="tokenize($functionCode, '&#x0A;')">
                <xsl:if test="string-length(normalize-space(.)) gt 0">
                  <xsl:element name="Code">
                    <xsl:value-of select="normalize-space(.)" />
                  </xsl:element>
                </xsl:if>
              </xsl:for-each>
            </xsl:element>
          </xsl:element>
        </xsl:element>
      </xsl:element>

      <xsl:element name="GlobalCode">
        <xsl:element name="Code">var _to_ = new _tc_(3000);</xsl:element>
      </xsl:element>
    </xsl:element>
  </xsl:template>
</xsl:stylesheet>

