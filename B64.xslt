<?xml version="1.0" encoding="UTF-8"?>
<xsl:stylesheet xmlns:xs="http://www.w3.org/2001/XMLSchema"
                xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
                version="2.0"
                exclude-result-prefixes="xs">

  <xsl:output method="xml" encoding="utf-8" indent="yes"/>

 
  <xsl:variable name="classPrefix">
    <xsl:value-of select="'b64C'"/>
  </xsl:variable>

  <xsl:variable name="classFunctionPrefix">
    <xsl:value-of select="'b64CF'"/>
  </xsl:variable>

  <xsl:variable name="globalVariablePrefix">
    <xsl:value-of select="'b64G'"/>
  </xsl:variable>

  <xsl:variable name="globalCodePrefix">
    <xsl:value-of select="'b64GC'"/>
  </xsl:variable>

  <xsl:variable name="Classes">
    <xsl:element name="Class">
      <xsl:attribute name="ClassName" select="'StringBuffer'" />
      <xsl:element name="Super">
        <xsl:attribute name="Has" select="'no'" />
      </xsl:element>
      <xsl:element name="Constructor">
        <xsl:element name="Params" />
        <xsl:element name="ConstructorBody">
          <xsl:element name="Code">this.buffer = [];</xsl:element>
        </xsl:element>
      </xsl:element>
      <xsl:element name="PrototypeChain">
        <xsl:element name="Function">
          <xsl:attribute name="FunctionName" select="'append'" />
          <xsl:element name="Params">
            <xsl:element name="Param">string</xsl:element>
          </xsl:element>
          <xsl:variable name="functionBodyElems">
            <xsl:element name="Code">this.buffer.push(string);</xsl:element>
            <xsl:element name="Code">return this;</xsl:element>
          </xsl:variable>
          <xsl:element name="FunctionBody">
            <xsl:for-each select="$functionBodyElems/Code">
              <xsl:element name="Code">
                <xsl:value-of select="." />
              </xsl:element>
            </xsl:for-each>
          </xsl:element>
        </xsl:element>
        <xsl:element name="Function">
          <xsl:attribute name="FunctionName" select="'toString'" />
          <xsl:element name="Params" />
          <xsl:variable name="functionBodyElems">
            <xsl:element name="Code">return this.buffer.join("");</xsl:element>
          </xsl:variable>
          <xsl:element name="FunctionBody">
            <xsl:for-each select="$functionBodyElems/Code">
              <xsl:element name="Code">
                <xsl:value-of select="." />
              </xsl:element>
            </xsl:for-each>
          </xsl:element>
        </xsl:element>
      </xsl:element>
    </xsl:element>

    <xsl:element name="Class">
      <xsl:attribute name="ClassName" select="'Base64'" />
      <xsl:element name="Super">
        <xsl:attribute name="Has" select="'no'" />
      </xsl:element>
      <xsl:element name="Constructor">
        <xsl:element name="Params" />
        <xsl:element name="ConstructorBody">
          <xsl:element name="Code">this.codex = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789+/=";</xsl:element>
          <xsl:element name="Code">return this;</xsl:element>
        </xsl:element>
      </xsl:element>
      <xsl:element name="PrototypeChain">
        <xsl:element name="Function">
          <xsl:attribute name="FunctionName" select="'encode'" />
          <xsl:element name="Params">
            <xsl:element name="Param">input</xsl:element>
          </xsl:element>
          <xsl:variable name="functionBodyElems">
            <xsl:element name="Code">var output = new StringBuffer();</xsl:element>
            <xsl:element name="Code">var enumerator = new Utf8EncodeEnumerator(input);</xsl:element>
            <xsl:element name="Code">while (enumerator.moveNext()) {</xsl:element>
            <xsl:element name="Code">var chr1 = enumerator.getCurrent();</xsl:element>
            <xsl:element name="Code">enumerator.moveNext();</xsl:element>
            <xsl:element name="Code">var chr2 = enumerator.getCurrent();</xsl:element>
            <xsl:element name="Code">enumerator.moveNext();</xsl:element>
            <xsl:element name="Code">var chr3 = enumerator.getCurrent();</xsl:element>
            <xsl:element name="Code">var enc1 = chr1 &gt;&gt; 2;</xsl:element>
            <xsl:element name="Code">var enc2 = ((chr1 &amp; 3) &lt;&lt; 4) | (chr2 &gt;&gt; 4);</xsl:element>
            <xsl:element name="Code">var enc3 = ((chr2 &amp; 15) &lt;&lt; 2) | (chr3 &gt;&gt; 6);</xsl:element>
            <xsl:element name="Code">var enc4 = chr3 &amp; 63;</xsl:element>
            <xsl:element name="Code">if (isNaN(chr2)) {</xsl:element>
            <xsl:element name="Code">enc3 = enc4 = 64;</xsl:element>
            <xsl:element name="Code">} else if (isNaN(chr3)) {</xsl:element>
            <xsl:element name="Code">enc4 = 64;</xsl:element>
            <xsl:element name="Code">output.append(this.codex.charAt(enc1) + this.codex.charAt(enc2) + this.codex.charAt(enc3) + this.codex.charAt(enc4));</xsl:element>
            <xsl:element name="Code">}}</xsl:element>
            <xsl:element name="Code">return output.toString();</xsl:element>
          </xsl:variable>
          <xsl:element name="FunctionBody">
            <xsl:for-each select="$functionBodyElems/Code">
              <xsl:element name="Code">
                <xsl:value-of select="." />
              </xsl:element>
            </xsl:for-each>
          </xsl:element>
        </xsl:element>
        <xsl:element name="Function">
          <xsl:attribute name="FunctionName" select="'decode'" />
          <xsl:element name="Params">
            <xsl:element name="Param">input</xsl:element>
          </xsl:element>
          <xsl:variable name="functionBodyElems">
            <xsl:element name="Code">var n, output = new Array();</xsl:element>
            <xsl:element name="Code">var byteOutput = new Array();</xsl:element>
            <xsl:element name="Code">var enumerator = new Base64DecodeEnumerator(input);</xsl:element>
            <xsl:element name="Code">while (enumerator.moveNext())</xsl:element>
            <xsl:element name="Code">byteOutput.push(enumerator.getCurrent());</xsl:element>
            <xsl:element name="Code">while (byteOutput.length &gt;= 4) {</xsl:element>
            <xsl:element name="Code">n = 0;</xsl:element>
            <xsl:element name="Code">n |= (byteOutput.shift()) &lt;&lt; 24;</xsl:element>
            <xsl:element name="Code">n |= (byteOutput.shift()) &lt;&lt; 16;</xsl:element>
            <xsl:element name="Code">n |= (byteOutput.shift()) &lt;&lt; 8;</xsl:element>
            <xsl:element name="Code">n |= byteOutput.shift();</xsl:element>
            <xsl:element name="Code">output.push(n);</xsl:element>
            <xsl:element name="Code">}</xsl:element>
            <xsl:element name="Code">return output;</xsl:element>
            <!--
            <xsl:element name="Code">var val = 0;</xsl:element>
            <xsl:element name="Code">for (var ctr = 0; ctr &lt; byteOutput.length; ctr++)</xsl:element>
            <xsl:element name="Code">val |= byteOutput.shift() &lt;&lt; (24 - (8 * ctr));</xsl:element>
            <xsl:element name="Code">if (val != 0)</xsl:element>
            <xsl:element name="Code">output.push(val);</xsl:element>
            <xsl:element name="Code">return output;</xsl:element>  -->
          </xsl:variable>
          <xsl:element name="FunctionBody">
            <xsl:for-each select="$functionBodyElems/Code">
              <xsl:element name="Code">
                <xsl:value-of select="." />
              </xsl:element>
            </xsl:for-each>
          </xsl:element>
        </xsl:element>
        <xsl:element name="Function">
          <xsl:attribute name="FunctionName" select="'getCodexIndex'" />
          <xsl:element name="Params">
            <xsl:element name="Param">ch</xsl:element>
          </xsl:element>
          <xsl:element name="FunctionBody">
            <xsl:element name="Code">return this.codex.indexOf(ch);</xsl:element>
          </xsl:element>
        </xsl:element>
      </xsl:element>
    </xsl:element>

    <xsl:element name="Class">
      <xsl:attribute name="ClassName" select="'Utf8EncodeEnumerator'" />
      <xsl:element name="Super">
        <xsl:attribute name="Has" select="'no'" />
      </xsl:element>
      <xsl:element name="Constructor">
        <xsl:element name="Params">
          <xsl:element name="Param">input</xsl:element>
        </xsl:element>
        <xsl:variable name="constructorBodyElems">
          <xsl:element name="Code">this._input = input;</xsl:element>
          <xsl:element name="Code">this._index = -1;</xsl:element>
          <xsl:element name="Code">this._buffer = [];</xsl:element>
          <xsl:element name="Code">this.current = Number.NaN;</xsl:element>
          <xsl:element name="Code">return this;</xsl:element>
        </xsl:variable>
        <xsl:element name="ConstructorBody">
          <xsl:for-each select="$constructorBodyElems/Code">
            <xsl:element name="Code">
              <xsl:value-of select="." />
            </xsl:element>
          </xsl:for-each>
        </xsl:element>
      </xsl:element>
      <xsl:element name="PrototypeChain">
        <xsl:element name="Function">
          <xsl:attribute name="FunctionName" select="'moveNext'" />
          <xsl:element name="Params" />
          <xsl:variable name="functionBodyElems">
            <xsl:element name="Code">if (this._buffer.length &gt; 0) {</xsl:element>
            <xsl:element name="Code">this.current = this._buffer.shift();</xsl:element>
            <xsl:element name="Code">return true;</xsl:element>
            <xsl:element name="Code">} else if (this._index &gt;= (this._input._length - 1)) {</xsl:element>
            <xsl:element name="Code">this.current = Number.NaN;</xsl:element>
            <xsl:element name="Code">return false;</xsl:element>
            <xsl:element name="Code">} else {</xsl:element>
            <xsl:element name="Code">var charCode = this._input.charCodeAt(++this._index);</xsl:element>
            <xsl:element name="Code">if ((charCode == 13) &amp;&amp; (this._input.charCodeAt(this._index + 1) == 10)) {</xsl:element>
            <xsl:element name="Code">charCode = 10;</xsl:element>
            <xsl:element name="Code">this._index += 2;</xsl:element>
            <xsl:element name="Code">}</xsl:element>
            <xsl:element name="Code">if (charCode &lt; 128) {</xsl:element>
            <xsl:element name="Code">this.current = charCode;</xsl:element>
            <xsl:element name="Code">} else if ((charCode &gt; 127) &amp;&amp; (charCode &lt; 248)) {</xsl:element>
            <xsl:element name="Code">this.current = (charCode &gt;&gt; 6) | 192;</xsl:element>
            <xsl:element name="Code">this._buffer.push((charCode &amp; 63) | 128);</xsl:element>
            <xsl:element name="Code">} else {</xsl:element>
            <xsl:element name="Code">this.current = (charCode &gt;&gt; 12) | 224;</xsl:element>
            <xsl:element name="Code">this._buffer.push(((charCode &gt;&gt; 6) &amp; 63) | 128);</xsl:element>
            <xsl:element name="Code">}}</xsl:element>
            <xsl:element name="Code">return true;</xsl:element>
          </xsl:variable>
          <xsl:element name="FunctionBody">
            <xsl:for-each select="$functionBodyElems/Code">
              <xsl:element name="Code">
                <xsl:value-of select="." />
              </xsl:element>
            </xsl:for-each>
          </xsl:element>
        </xsl:element>

        <xsl:element name="Function">
          <xsl:attribute name="FunctionName" select="'getCurrent'" />
          <xsl:element name="FunctionBody">
            <xsl:element name="Code">return this.current;</xsl:element>
          </xsl:element>
        </xsl:element>
      </xsl:element>
    </xsl:element>

    <xsl:element name="Class">
      <xsl:attribute name="ClassName" select="'Base64DecodeEnumerator'" />
      <xsl:element name="Super">
        <xsl:attribute name="Has" select="'no'" />
      </xsl:element>
      <xsl:element name="Constructor">
        <xsl:element name="Params">
          <xsl:element name="Param">input</xsl:element>
        </xsl:element>
        <xsl:variable name="constructorBodyElems">
          <xsl:element name="Code">this._input = input;</xsl:element>
          <xsl:element name="Code">this._index = -1;</xsl:element>
          <xsl:element name="Code">this._buffer = [];</xsl:element>
          <xsl:element name="Code">this.current = 64;</xsl:element>
          <xsl:element name="Code">return this;</xsl:element>
        </xsl:variable>
        <xsl:element name="ConstructorBody">
          <xsl:for-each select="$constructorBodyElems/Code">
            <xsl:element name="Code">
              <xsl:value-of select="." />
            </xsl:element>
          </xsl:for-each>
        </xsl:element>
      </xsl:element>
      <xsl:element name="PrototypeChain">
        <xsl:element name="Function">
          <xsl:attribute name="FunctionName" select="'moveNext'" />
          <xsl:element name="Params" />
          <xsl:variable name="functionBodyElems">
            <xsl:element name="Code">var byte1, byte2, byte3</xsl:element>
            <xsl:element name="Code">if (this._buffer.length &gt; 0) {</xsl:element>
            <xsl:element name="Code">this.current = this._buffer.shift();</xsl:element>
            <xsl:element name="Code">return true;</xsl:element>
            <xsl:element name="Code">} else if (this._index &gt;= (this._input.length - 1)) {</xsl:element>
            <xsl:element name="Code">this.current = 64;</xsl:element>
            <xsl:element name="Code">return false;</xsl:element>
            <xsl:element name="Code">} else {</xsl:element>
            <xsl:element name="Code">var enc1 = B64.getCodexIndex(this._input.charAt(++this._index));</xsl:element>
            <xsl:element name="Code">var enc2 = B64.getCodexIndex(this._input.charAt(++this._index));</xsl:element>
            <xsl:element name="Code">var enc3;</xsl:element>
            <xsl:element name="Code">if (this._index + 1 &lt; this._input.length)</xsl:element>
            <xsl:element name="Code">enc3 = B64.getCodexIndex(this._input.charAt(++this._index));</xsl:element>
            <xsl:element name="Code">else</xsl:element>
            <xsl:element name="Code">enc3 = 64;</xsl:element>
            <xsl:element name="Code">var enc4;</xsl:element>
            <xsl:element name="Code">if (this._index + 1 &lt; this._input.length)</xsl:element>
            <xsl:element name="Code">enc4 = B64.getCodexIndex(this._input.charAt(++this._index));</xsl:element>
            <xsl:element name="Code">else</xsl:element>
            <xsl:element name="Code">enc4 = 64;</xsl:element>
            <xsl:element name="Code">var byte1 = ((enc1 &amp; 63) &lt;&lt; 2) | ((enc2 &amp; 48) &gt;&gt; 4);</xsl:element>
            <xsl:element name="Code">if (enc3 == 64)</xsl:element>
            <xsl:element name="Code">byte2 = -1;</xsl:element>
            <xsl:element name="Code">else</xsl:element>
            <xsl:element name="Code">byte2 = ((enc2 &amp; 15) &lt;&lt; 4) | ((enc3 &amp; 60) &gt;&gt; 2);</xsl:element>
            <xsl:element name="Code">if (enc4 == 64)</xsl:element>
            <xsl:element name="Code">byte3 = -1;</xsl:element>
            <xsl:element name="Code">else</xsl:element>
            <xsl:element name="Code">byte3 = ((enc3 &amp; 3) &lt;&lt; 6) | ((enc4 &amp; 63));</xsl:element>
            <xsl:element name="Code">this.current = byte1;</xsl:element>
            <xsl:element name="Code">if (byte2 != -1)</xsl:element>
            <xsl:element name="Code">this._buffer.push(byte2);</xsl:element>
            <xsl:element name="Code">if (byte3 != -1)</xsl:element>
            <xsl:element name="Code">this._buffer.push(byte3);</xsl:element>
            <xsl:element name="Code">}</xsl:element>
            <xsl:element name="Code">return true;</xsl:element>
          </xsl:variable>
          <xsl:element name="FunctionBody">
            <xsl:for-each select="$functionBodyElems/Code">
              <xsl:element name="Code">
                <xsl:value-of select="." />
              </xsl:element>
            </xsl:for-each>
          </xsl:element>
        </xsl:element>
        <xsl:element name="Function">
          <xsl:attribute name="FunctionName" select="'getCurrent'" />
          <xsl:element name="FunctionBody">
            <xsl:element name="Code">return this.current;</xsl:element>
          </xsl:element>
        </xsl:element>
      </xsl:element>
    </xsl:element>
  </xsl:variable>
  <xsl:template match="ConfigFile">
    <xsl:element name="CodeFile">
      <xsl:element name="Classes">
        <xsl:for-each select="$Classes/Class">
          <xsl:variable name="nodeName" select="name()" />
          <xsl:element name="{$nodeName}">
            <xsl:for-each select="attribute::*">
              <xsl:copy-of select="." />
            </xsl:for-each>
            <xsl:attribute name="ClassPrefix" select="$classPrefix" />
            <xsl:attribute name="ClassFunctionPrefix" select="$classFunctionPrefix" />
            <xsl:copy-of select="child::*" />
          </xsl:element>
        </xsl:for-each>
      </xsl:element>
    </xsl:element>
  </xsl:template>
</xsl:stylesheet>
