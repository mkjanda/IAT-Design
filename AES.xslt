<?xml version="1.0" encoding="UTF-8"?>
<xsl:stylesheet xmlns:xs="http://www.w3.org/2001/XMLSchema"
                xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
                version="2.0"
                exclude-result-prefixes="xs">

  <xsl:output method="xml" encoding="utf-8" indent="yes"/>

  <xsl:variable name="VariableDeclarations">
    <Declarations>
      <Declaration>var B64;</Declaration>
      <Declaration>var decryptor;</Declaration>
    </Declarations>
  </xsl:variable>

  <xsl:variable name="classPrefix">
    <xsl:value-of select="'aesC'"/>
  </xsl:variable>

  <xsl:variable name="classFunctionPrefix">
    <xsl:value-of select="'aesCF'"/>
  </xsl:variable>

  <xsl:variable name="globalVariablePrefix">
    <xsl:value-of select="'aesG'"/>
  </xsl:variable>

  <xsl:variable name="globalCodePrefix">
    <xsl:value-of select="'aesGC'"/>
  </xsl:variable>

  <xsl:variable name="GlobalAbbreviations">
    <xsl:variable name="Globals" select="string-join(for $elem in $VariableDeclarations/Declarations/Declaration return replace($elem, '^var\s+(.+);$', '$1,,'), '')" />
    <xsl:analyze-string select="$Globals" regex="([A-Za-z_][A-Za-z0-9_]*)(\s*=\s*(\[|\s+|[^,;=/&#34;\(\[\]]+|(&#34;[^&#xA;&#xD;&#34;]*?&#34;)+|\(([^;,=&#34;]*(,)?(&#34;[^&#xA;&#xD;&#34;]*?&#34;)?)*?\)|/[^/\n]+?/|\](\s*,)?)+)?,,">
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
            <xsl:value-of select="regex-group(2)" />
          </xsl:element>
        </xsl:element>
      </xsl:matching-substring>
    </xsl:analyze-string>
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
    <xsl:element name="Class">
      <xsl:attribute name="ClassName" select="'Decryptor'" />
      <xsl:element name="Super">
        <xsl:attribute name="Has" select="'no'" />
      </xsl:element>
      <xsl:element name="Constructor">
        <xsl:element name="Params">
          <xsl:element name="Param">ajaxURL</xsl:element>
          <xsl:element name="Param">serverContext</xsl:element>
          <xsl:element name="Param">onDone</xsl:element>
        </xsl:element>
        <xsl:variable name="constructorBodyElems">
          <xsl:element name="Code">decryptor = this;</xsl:element>
          <xsl:element name="Code">this.Keys = null</xsl:element>
          <xsl:element name="Code">if (onDone)</xsl:element>
          <xsl:element name="Code">this.OnDone = onDone;</xsl:element>
          <xsl:element name="Code">else</xsl:element>
          <xsl:element name="Code">this.OnDone = null;</xsl:element>
          <xsl:element name="Code">
            <xsl:value-of select="concat('this.ClientID = ', //ClientID, ';')"/>
          </xsl:element>
          <xsl:element name="Code">
            <xsl:value-of select="concat('this.TestName = &quot;', //IATName, '&quot;;')" />
          </xsl:element>
          <xsl:element name="Code">this.ServerContext = serverContext;</xsl:element>
          <xsl:element name="Code">this.AjaxURL = ajaxURL;</xsl:element>
          <xsl:element name="Code">this.Lines = new Array();</xsl:element>
          <xsl:element name="Code">this.RootURL = window.location.protocol + "//" + window.location.hostname + (window.location.port ? ":" + window.location.port.toString() : "") + "/" + this.ServerContext + "/";</xsl:element>
          <xsl:element name="Code">this.RequestSrc = window.location.protocol + "//" + window.location.hostname + (window.location.port ? ":" + window.location.port.toString() : "") + "/" + this.ServerContext + "/" + this.ClientID.toString() + "/" + this.TestName + "/";</xsl:element>
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
          <xsl:attribute name="FunctionName" select="'fetchKeys'" />
          <xsl:element name="Params">
            <xsl:element name="Param">segmentID</xsl:element>
            <xsl:element name="Param">ajaxURL</xsl:element>
          </xsl:element>
          <xsl:element name="FunctionBody">
            <xsl:element name="Code">this.segmentID = segmentID;</xsl:element>
            <xsl:element name="Code">this.fetchPublicKey(ajaxURL);</xsl:element>
          </xsl:element>
        </xsl:element>

        <xsl:element name="Function">
          <xsl:attribute name="FunctionName" select="'fetchPublicKey'" />
          <xsl:element name="Params">
            <xsl:element name="Param">ajaxURL</xsl:element>
          </xsl:element>
          <xsl:variable name="functionCode">
            <xsl:text>
            var ajaxCall = new AjaxCall(ajaxURL, this.RootURL, "", this.segmentID);
            var verFile = new Object();
            verFile.filename = "AES.dat";
            verFile.relTo = "Test";
            verFile.sourceType= "File";
            var xmlDoc = ajaxCall.buildRequestDocument("Keys", "", "PublicKey");
            ajaxCall.call(xmlDoc, this.retrieveKeys, this);
            </xsl:text>
          </xsl:variable>
          <xsl:element name="FunctionBody">
            <xsl:for-each select="tokenize($functionCode, '&#x0A;')">
              <xsl:if test="string-length(normalize-space(.)) gt 0">
                <xsl:element name="Code">
                  <xsl:value-of select="normalize-space(.)"/>
                </xsl:element>
              </xsl:if>
            </xsl:for-each>
          </xsl:element>
        </xsl:element>

        <xsl:element name="Function">
          <xsl:attribute name="FunctionName" select="'retrieveKeys'" />
          <xsl:element name="Params">
            <xsl:element name="Param">ajaxResult</xsl:element>
          </xsl:element>
          <xsl:variable name="functionCode">
            <xsl:text>
              this.RSA = eval(ajaxResult);
              var keyCipherString = "";
              var randHexVal, hexVal, hexStr, ctr1, ctr2, ctr3, keyStr;
              var keyBytes = new Array();
              this.KeyCipherWords = new Array();
              for (ctr1 = 0; ctr1 &lt; 4; ctr1++) {
                keyStr = "";
                for (ctr2 = 0; ctr2 &lt; 4; ctr2++) {
                  randHexVal = Math.floor(Math.random() * 256);
                  hexStr = randHexVal.toString(16);
                  while (hexStr.length &lt; 2)
                    hexStr = "0" + hexStr;
                  keyStr += hexStr;
                  hexVal = 1;
                  for (ctr3 = 0; ctr3 &lt; this.RSA.exponent; ctr3++)
                    hexVal = (hexVal * randHexVal) % this.RSA.modulus;
                  keyBytes.push(hexVal.toString(16));
                }
                this.KeyCipherWords.push(parseInt(keyStr, 16));
              }
            </xsl:text>
            <xsl:value-of select="concat('var ajaxURL = window.location.protocol + &quot;//&quot; + window.location.hostname + (window.location.port ? &quot;:&quot; + window.location.port.toString() : &quot;&quot;) + &quot;', //ServerPath, '/Admin/Ajax/JSON&quot;;')" />
            <xsl:text>
              var ajaxCall = new AjaxCall(ajaxURL, this.RootURL, "", this.segmentID);
              var byteStr = keyBytes.join("|");
              var xmlDoc = ajaxCall.buildRequestDocument("Keys", byteStr, "JSON");
              ajaxCall.call(xmlDoc, this.processKeys, this);
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

        <xsl:element name="Function">
          <xsl:attribute name="FunctionName" select="'decryptKeys'" />
          <xsl:element name="Params">
            <xsl:element name="Param">keySet</xsl:element>
          </xsl:element>
          <xsl:variable name="functionCode">
            <xsl:text>
      var keyAES = new AES(this.KeyCipherWords);
      var aesAry = new Array();
      var decOut = new Array();
      var ctr = 0;
      for (ctr = 0; ctr &lt; keySet.length; ctr++) {
        decOut = keyAES.decrypt(new Array(keySet[ctr].keyWords[0], keySet[ctr].keyWords[1], keySet[ctr].keyWords[2], keySet[ctr].keyWords[3]));
        decOut = decOut.concat(keyAES.decrypt(new Array(keySet[ctr].keyWords[4], keySet[ctr].keyWords[5], keySet[ctr].keyWords[6], keySet[ctr].keyWords[7])));
        aesAry.push(new AES(decOut));
      }
      return aesAry;
            </xsl:text>
          </xsl:variable>
          <xsl:element name="FunctionBody">
            <xsl:for-each select="tokenize($functionCode, '&#x0A;')">
              <xsl:if test="string-length(normalize-space(.)) gt 0">
                <xsl:element name="Code">
                  <xsl:value-of select="normalize-space(.)"/>
                </xsl:element>
              </xsl:if>
            </xsl:for-each>
          </xsl:element>
        </xsl:element>

        <xsl:element name="Function">
          <xsl:attribute name="FunctionName" select="'processKeys'" />
          <xsl:element name="Params">
            <xsl:element name="Param">ajaxResult</xsl:element>
          </xsl:element>
          <xsl:variable name="functionCode">
            <xsl:text>
              var encryptedKeys = eval("(" + ajaxResult + ")");
              this.AESAry = this.decryptKeys(encryptedKeys);
            </xsl:text>
            <xsl:value-of select="concat('var ajaxURL = window.location.protocol + &quot;//&quot; + window.location.hostname + (window.location.port ? &quot;:&quot; + window.location.port.toString() : &quot;&quot;) + &quot;', //ServerPath, '/Admin/Ajax/Code&quot;;')" />
            <xsl:text>
              var ajaxCall = new AjaxCall(ajaxURL, this.RootURL, this.RequestSrc + this.TestElem + ".html", this.segmentID);
              var xmlDoc = ajaxCall.buildRequestDocument(this.TestElem, this.KeyCipherString, "Code");
              ajaxCall.call(xmlDoc, this.processCode, this);
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

        <xsl:element name="Function">
          <xsl:attribute name="FunctionName" select="'processCode'" />
          <xsl:element name="Params">
            <xsl:element name="Param">ajaxDoc</xsl:element>
          </xsl:element>
          <xsl:variable name="functionCode">
            <xsl:text>
      var ctr, declLine, tocLine, globalLine;
      var lines = ajaxDoc.getElementsByTagName("Line");
      for (ctr = 0; ctr &lt; lines.length; ctr++) {
        if (lines[ctr].getAttribute("Type") == "TOC") {
          tocLine = this.decryptNode({ "CL" : parseInt(lines[ctr].getAttribute("CL"), 10), "ANDX" : parseInt(lines[ctr].getAttribute("ANDX"), 10), "BNDX" : parseInt(lines[ctr].getAttribute("BNDX"), 10), "SegmentNodes" : lines[ctr].childNodes });
          eval.call(window, tocLine);
        }
      }
      for (ctr = 0; ctr &lt; lines.length; ctr++) {
        if (lines[ctr].getAttribute("Type") == "Code") 
          this.Lines.push({ "CL" : parseInt(lines[ctr].getAttribute("CL"), 10), "ANDX" : parseInt(lines[ctr].getAttribute("ANDX"), 10), "BNDX" : parseInt(lines[ctr].getAttribute("BNDX"), 10), "SegmentNodes" : lines[ctr].childNodes });
      }
      for (ctr = 0; ctr &lt; lines.length; ctr++) {
        if ((lines[ctr].getAttribute("Type") == "Constructor") || (lines[ctr].getAttribute("Type") == "Declaration")) {
          declLine = this.decryptNode({ "CL" : parseInt(lines[ctr].getAttribute("CL"), 10), "ANDX" : parseInt(lines[ctr].getAttribute("ANDX"), 10), "BNDX" : parseInt(lines[ctr].getAttribute("BNDX"), 10), "SegmentNodes" : lines[ctr].childNodes });
          eval.call(window, declLine);
        }
      }
      for (ctr = 0; ctr &lt; lines.length; ctr++) {
        if ((lines[ctr].getAttribute("Type") == "GlobalDeclaration") || (lines[ctr].getAttribute("Type") == "GlobalCode")) {
          globalLine = this.decryptNode({ "CL" : parseInt(lines[ctr].getAttribute("CL"), 10), "ANDX" : parseInt(lines[ctr].getAttribute("ANDX"), 10), "BNDX" : parseInt(lines[ctr].getAttribute("BNDX"), 10), "SegmentNodes" : lines[ctr].childNodes });
          eval.call(window, globalLine);
        }
      }

      if (this.OnDone != null)
        this.OnDone.call();
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

        <xsl:element name="Function">
          <xsl:attribute name="FunctionName" select="'decryptNode'" />
          <xsl:element name="Params">
            <xsl:element name="Param">lineXML</xsl:element>
          </xsl:element>
          <xsl:variable name="functionCode">
            <xsl:text>
      return this.decryptLine(lineXML.CL, lineXML.ANDX, lineXML.BNDX, lineXML.SegmentNodes);
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

        <xsl:element name="Function">
          <xsl:attribute name="FunctionName" select="'decryptLine'" />
          <xsl:element name="Params">
            <xsl:element name="Param">cl</xsl:element>
            <xsl:element name="Param">andx</xsl:element>
            <xsl:element name="Param">bndx</xsl:element>
            <xsl:element name="Param">segmentNodes</xsl:element>
          </xsl:element>
          <xsl:variable name="functionCode">
            <xsl:text>
      var aesNum = andx ^ bndx;
      var ctr, ctr2, codeNode, code;
      var encCode = new Array();
      for (ctr = 0; ctr &lt; 4; ctr++) {
        codeNode = segmentNodes[ctr].firstChild;
        code = &quot;&quot;;
        while (codeNode) {
          code += codeNode.childNodes[0].nodeValue;
          codeNode = codeNode.nextSibling;
        }
        encCode.push(code);
      }
      var segments = [ B64.decode(encCode[0]), B64.decode(encCode[1]), B64.decode(encCode[2]), B64.decode(encCode[3]) ];
      var wordSegments = new Array();
      for (ctr = 0; ctr &lt; 4; ctr++)
        wordSegments.push(new Array());
</xsl:text>
            <!--
      for (ctr = 0; ctr &lt; segments[0].length; ctr += 4) {
        wordSegments[0] = wordSegments[0].concat(segments[0][ctr], segments[0][ctr+1], segments[0][ctr+2], segments[0][ctr+3]);
        wordSegments[1] = wordSegments[1].concat(segments[1][ctr], segments[1][ctr+1], segments[1][ctr+2], segments[1][ctr+3]);
        wordSegments[2] = wordSegments[2].concat(segments[2][ctr], segments[2][ctr+1], segments[2][ctr+2], segments[2][ctr+3]);
        wordSegments[3] = wordSegments[3].concat(segments[3][ctr], segments[3][ctr+1], segments[3][ctr+2], segments[3][ctr+3]);
      }-->
            <xsl:text>
      var segLen = segments[0].length;
      var outWordAry = new Array();
      for (ctr = 0; ctr &lt; segLen; ctr++)  {
        var inWordAry = [ segments[0][ctr], segments[1][ctr], segments[2][ctr], segments[3][ctr]];
        outWordAry = outWordAry.concat(this.AESAry[aesNum].decrypt(inWordAry));
      }
      var result = new String();
      var segCtr = 0, wordCtr = 0;
      for (ctr = 0; ctr &lt; (cl &gt;&gt; 2) + 1; ctr++) {
        var w = outWordAry[ctr];
        for (ctr2 = 0; ctr2 &lt; 4; ctr2++) {
          if ((ctr &lt;&lt; 2) + ctr2 &lt; cl)
            result += String.fromCharCode((w &amp; (0xFF &lt;&lt; (24 - (8 * ctr2)))) >>> (24 - (8 * ctr2)));
        }
      }            
      return result;
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

        <xsl:element name="Function">
          <xsl:attribute name="FunctionName" select="'getTOCEntry'" />
          <xsl:element name="Params">
            <xsl:element name="Param">name</xsl:element>
          </xsl:element>
          <xsl:variable name="functionCode">
            <xsl:text>
              return TOC[name];
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

        <xsl:element name="Function">
          <xsl:attribute name="FunctionName" select="'getLine'" />
          <xsl:element name="Params">
            <xsl:element name="Param">ndx</xsl:element>
          </xsl:element>
          <xsl:variable name="functionCode">
            <xsl:text>
              return this.Lines[ndx];
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

    <xsl:element name="Class">
      <xsl:attribute name="ClassName" select="'SubFunct'" />
      <xsl:element name="Super">
        <xsl:attribute name="Has" select="'no'" />
      </xsl:element>
      <xsl:element name="Constructor">
        <xsl:element name="Params">
          <xsl:element name="Param">name</xsl:element>
          <xsl:element name="Param">childFunct</xsl:element>
        </xsl:element>
        <xsl:variable name="constructorCode">
          <xsl:text>
    this.Evaluated = false;
    this.Decryptor = decryptor;
    this.ndx1 = TOC[name].ndx1;
    this.ndx2 = TOC[name].ndx2;
    this.ChildFunct = childFunct;
    return this;
  </xsl:text>
        </xsl:variable>
        <xsl:element name="ConstructorBody">
          <xsl:for-each select="tokenize($constructorCode, '&#x0A;')">
            <xsl:if test="string-length(normalize-space(.)) gt 0">
              <xsl:element name="Code">
                <xsl:value-of select="normalize-space(.)" />
              </xsl:element>
            </xsl:if>
          </xsl:for-each>
        </xsl:element>
      </xsl:element>

      <xsl:element name="PrototypeChain">
        <xsl:element name="Function">
          <xsl:attribute name="FunctionName" select="'fEval'" />
          <xsl:element name="Params">
            <xsl:element name="Param">param</xsl:element>
          </xsl:element>
          <xsl:variable name="functionCode">
            <xsl:text>
<!--          _to_.refresh();  -->
            var f;
            var code = this.Decryptor.decryptNode(this.Decryptor.getLine(this.ndx1 ^ this.ndx2));
            if (this.ChildFunct)
              f = eval("(function(_l) { " + code + " })");
            else           
              f = eval("(function(_p) { " + code + " })");
          
          return f.call(window, param);
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

        <xsl:element name="Function">
          <xsl:attribute name="FunctionName" select="'cEval'" />
          <xsl:element name="Params">
            <xsl:element name="Param">t</xsl:element>
            <xsl:element name="Param">param</xsl:element>
          </xsl:element>
          <xsl:variable name="functionCode">
            <xsl:text>
<!--          _to_.refresh(); -->
            var code = this.Decryptor.decryptNode(this.Decryptor.getLine(this.ndx1 ^ this.ndx2));
            var f;
            if (this.ChildFunct) 
              f = eval("(function(_l) { " + code + " })");
            else 
              f = eval("(function(_p) { " + code + " })");
          
          return f.call(t, param);
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
  </xsl:variable>

  <xsl:variable name="GlobalCode">
    <xsl:element name="Code">B64 = new Base64();</xsl:element>
  </xsl:variable>

  <xsl:template match="ConfigFile">
    <xsl:element name="CodeFile">
      <xsl:element name="VarEntries">
        <xsl:copy-of select="$GlobalAbbreviations" />
      </xsl:element>
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
      <xsl:element name="GlobalCode">
        <xsl:attribute name="CodePrefix" select="$globalCodePrefix" />
        <xsl:copy-of select="$GlobalCode" />
      </xsl:element>
    </xsl:element>
  </xsl:template>
</xsl:stylesheet>

