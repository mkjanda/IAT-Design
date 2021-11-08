<?xml version="1.0" encoding="UTF-8"?>
<xsl:stylesheet xmlns:xs="http://www.w3.org/2001/XMLSchema"
                xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
                version="2.0"
                exclude-result-prefixes="xs">

  <xsl:output method="xml" encoding="utf-8" indent="yes"/>

  <xsl:variable name="VariableDeclarations">
    <Declarations>
      <Declaration>var inPracticeBlock = false;</Declaration>
      <Declaration>var currentItemKeyedDir = "Left";</Declaration>
      <Declaration>var currentContinueKeyCode = 32;</Declaration>
      <Declaration>var isErrorMarked = false;</Declaration>
      <Declaration>var currentHandler = null;</Declaration>
      <Declaration>var currentStimulus = null;</Declaration>
      <Declaration>var currentItemNum = 0;</Declaration>
      <Declaration>var currentItemID;</Declaration>
      <Declaration>var EventList = new Array();</Declaration>
      <Declaration>var EventCtr = 0;</Declaration>
      <Declaration>var ErrorMark;</Declaration>
      <Declaration>var ImageLoadStatusTextElement;</Declaration>
      <Declaration>var ClickToStartElement;</Declaration>
      <Declaration>var ClickToStartText;</Declaration>
      <Declaration>var KeyedDir;</Declaration>
      <Declaration>var KeyedDirArray;</Declaration>
      <Declaration>var OriginatingBlockArray;</Declaration>
      <Declaration>var StimulusIDArray;</Declaration>
      <Declaration>var ItemNumArray;</Declaration>
      <Declaration>var randomNum;</Declaration>
      <Declaration>var RandomItem;</Declaration>
      <Declaration>var SelectedItem;</Declaration>
      <Declaration>var KeyedDirInput;</Declaration>
      <Declaration>var MaskItemTrueArray;</Declaration>
      <Declaration>var Display;</Declaration>
      <Declaration>var MaskItemFalseArray;</Declaration>
      <Declaration>var DefaultKey;</Declaration>
      <Declaration>var FreeItemIDs;</Declaration>
      <Declaration>var Items;</Declaration>
      <Declaration>var Items1;</Declaration>
      <Declaration>var Items2;</Declaration>
      <Declaration>var ctr;</Declaration>
      <Declaration>var SelectionStimulusArray;</Declaration>
      <Declaration>var SelectionKeyedDir;</Declaration>
      <Declaration>var SelectionOriginatingBlock;</Declaration>
      <Declaration>var SelectionStimulus;</Declaration>
      <Declaration>var SelectionItemNum;</Declaration>
      <xsl:for-each select="./DisplayItemList/*">
        <Declaration>
          <xsl:value-of select="concat('var DisplayItem', ./ID, ';')"/>
        </Declaration>
      </xsl:for-each>
      <Declaration>var itemBlock;</Declaration>
      <Declaration>var instructionBlock;</Declaration>
      <Declaration>var ItemBlocks;</Declaration>
      <Declaration>var InstructionBlocks;</Declaration>
      <Declaration>var alternate;</Declaration>
      <Declaration>var itemBlockCtr;</Declaration>
      <Declaration>var instructionsBlockCtr;</Declaration>
      <Declaration>var numAlternatedItemBlocks;</Declaration>
      <Declaration>var numAlternatedInstructionBlocks;</Declaration>
    </Declarations>
  </xsl:variable>

  <xsl:variable name="EventUtilDefinition">
    <xsl:element name="Function">
      <xsl:attribute name="FunctionName" select="'addHandler'"/>
      <xsl:element name="Params">
        <xsl:element name="Param">element</xsl:element>
        <xsl:element name="Param">type</xsl:element>
        <xsl:element name="Param">handler</xsl:element>
      </xsl:element>
      <xsl:variable name="functionBodyElems">
        <xsl:element name="CodeLine">if (element.addEventListener) {</xsl:element>
        <xsl:element name="CodeLine">element.addEventListener(type, handler, false);</xsl:element>
        <xsl:element name="CodeLine">} else if (element.attachEvent) {</xsl:element>
        <xsl:element name="CodeLine">element.attachEvent("on" + type, handler);</xsl:element>
        <xsl:element name="CodeLine">} else {</xsl:element>
        <xsl:element name="CodeLine">element["on" + type] = handler;</xsl:element>
        <xsl:element name="CodeLine">}</xsl:element>
      </xsl:variable>
      <xsl:element name="FunctionBody">
        <xsl:for-each select="$functionBodyElems/CodeLine">
          <xsl:element name="CodeLine">
            <xsl:attribute name="LineNum" select="count(preceding-sibling::node())"/>
            <xsl:value-of select="."/>
          </xsl:element>
        </xsl:for-each>
      </xsl:element>
    </xsl:element>

    <xsl:element name="Function">
      <xsl:attribute name="FunctionName" select="'getEvent'"/>
      <xsl:element name="Params">
        <xsl:element name="Param">event</xsl:element>
      </xsl:element>
      <xsl:variable name="functionBodyElems">
        <xsl:element name="CodeLine">return event ? event : window.event;</xsl:element>
      </xsl:variable>
      <xsl:element name="FunctionBody">
        <xsl:for-each select="$functionBodyElems/CodeLine">
          <xsl:element name="CodeLine">
            <xsl:attribute name="LineNum" select="count(preceding-sibling::node())"/>
            <xsl:value-of select="."/>
          </xsl:element>
        </xsl:for-each>
      </xsl:element>

    </xsl:element>

    <xsl:element name="Function">
      <xsl:attribute name="FunctionName" select="'getTarget'"/>
      <xsl:element name="Params">
        <xsl:element name="Param">event</xsl:element>
      </xsl:element>
      <xsl:variable name="functionBodyElems">
        <xsl:element name="CodeLine">return event.target || event.srcElement;</xsl:element>
      </xsl:variable>
      <xsl:element name="FunctionBody">
        <xsl:for-each select="$functionBodyElems/CodeLine">
          <xsl:element name="CodeLine">
            <xsl:attribute name="LineNum" select="count(preceding-sibling::node())"/>
            <xsl:value-of select="."/>
          </xsl:element>
        </xsl:for-each>
      </xsl:element>
    </xsl:element>

    <xsl:element name="Function">
      <xsl:attribute name="FunctionName" select="'preventDefault'"/>
      <xsl:element name="Params">
        <xsl:element name="Param">event</xsl:element>
      </xsl:element>
      <xsl:variable name="functionBodyElems">
        <xsl:element name="Code"> if(event.preventDefault) {</xsl:element>
        <xsl:element name="Code">event.preventDefault();</xsl:element>
        <xsl:element name="Code">} else {</xsl:element>
        <xsl:element name="Code">event.returnValue = false;</xsl:element>
        <xsl:element name="Code">}</xsl:element>
      </xsl:variable>
      <xsl:element name="FunctionBody">
        <xsl:for-each select="$functionBodyElems/Code">
          <xsl:element name="CodeLine">
            <xsl:attribute name="LineNum" select="count(preceding-sibling::node())"/>
            <xsl:value-of select="."/>
          </xsl:element>
        </xsl:for-each>
      </xsl:element>
    </xsl:element>

    <xsl:element name="Function">
      <xsl:attribute name="FunctionName" select="'removeHandler'"/>
      <xsl:element name="Params">
        <xsl:element name="Param">element</xsl:element>
        <xsl:element name="Param">type</xsl:element>
        <xsl:element name="Param">handler</xsl:element>
      </xsl:element>
      <xsl:variable name="functionBodyElems">
        <xsl:element name="Code">if element.removeEventListener) {</xsl:element>
        <xsl:element name="Code">element.removeEventListener(type, handler, false);</xsl:element>
        <xsl:element name="Code">} else if (element.detachEvent) {</xsl:element>
        <xsl:element name="Code">element.detachEvent("on" + type, handler);</xsl:element>
        <xsl:element name="Code">} else {</xsl:element>
        <xsl:element name="Code">element["on" + type] = null;</xsl:element>
        <xsl:element name="Code">}</xsl:element>
      </xsl:variable>
      <xsl:element name="FunctionBody">
        <xsl:for-each select="$functionBodyElems/Code">
          <xsl:element name="CodeLine">
            <xsl:attribute name="LineNum" select="count(preceding-sibling::node())"/>
            <xsl:value-of select="."/>
          </xsl:element>
        </xsl:for-each>
      </xsl:element>
    </xsl:element>

    <xsl:element name="Function">
      <xsl:attribute name="FunctionName" select="'stopPropogation'"/>
      <xsl:element name="Params">
        <xsl:element name="Param">event</xsl:element>
      </xsl:element>
      <xsl:variable name="functionBodyElems">
        <xsl:element name="Code">if (event.stopPropagation) {</xsl:element>
        <xsl:element name="Code">event.stopPropagation();</xsl:element>
        <xsl:element name="Code">} else {</xsl:element>
        <xsl:element name="Code">event.cancelBubble = true;</xsl:element>
        <xsl:element name="Code">}</xsl:element>
      </xsl:variable>
      <xsl:element name="FunctionBody">
        <xsl:for-each select="$functionBodyElems/Code">
          <xsl:element name="CodeLine">
            <xsl:attribute name="LineNum" select="count(preceding-sibling::node())"/>
            <xsl:value-of select="."/>
          </xsl:element>
        </xsl:for-each>
      </xsl:element>
    </xsl:element>

    <xsl:element name="Function">
      <xsl:attribute name="FunctionName" select="'getCharCode'"/>
      <xsl:element name="Params">
        <xsl:element name="Param">event</xsl:element>
      </xsl:element>
      <xsl:variable name="functionBodyElems">
        <xsl:element name="Code">if (typeof event.charCode == "number") {</xsl:element>
        <xsl:element name="Code">return event.charCode;</xsl:element>
        <xsl:element name="Code">} else {</xsl:element>
        <xsl:element name="Code">return event.keyCode;</xsl:element>
        <xsl:element name="Code">}</xsl:element>
      </xsl:variable>
      <xsl:element name="FunctionBody">
        <xsl:for-each select="$functionBodyElems/Code">
          <xsl:element name="CodeLine">
            <xsl:attribute name="LineNum" select="count(preceding-sibling::node())"/>
            <xsl:value-of select="."/>
          </xsl:element>
        </xsl:for-each>
      </xsl:element>
    </xsl:element>
  </xsl:variable>

  <xsl:template name="OutputEventUtil">
    <xsl:element name="CodeLine">
      <xsl:attribute name="Type" select="'EventUtilStart'"/>
      <xsl:value-of select="'EventUtil = {'" />
    </xsl:element>
    <xsl:for-each select="$EventUtilDefinition/Function">
      <xsl:variable name="params" select="string-join(Params/Param, ', ')"/>
      <xsl:element name="CodeLine">
        <xsl:attribute name="Type" select="'EventUtilFunctionStart'"/>
        <xsl:attribute name="Name" select="@FunctionName"/>
        <xsl:value-of select="concat(@FunctionName, ' : function(', $params, ') {')"/>
      </xsl:element>
      <xsl:for-each select="FunctionBody/CodeLine">
        <xsl:element name="CodeLine">
          <xsl:attribute name="LineNum" select="position()"/>
          <xsl:value-of select="."/>
        </xsl:element>
      </xsl:for-each>
      <xsl:if test="position() lt last()">
        <xsl:element name="CodeLine">
          <xsl:attribute name="LineNum" select="position()"/>
          <xsl:attribute name="Type" select="'EventUtilFunctionEnd'"/>
          <xsl:value-of select="'}, '"/>
        </xsl:element>
      </xsl:if>
    </xsl:for-each>
    <xsl:element name="CodeLine">
      <xsl:attribute name="Type" select="'EventUtilEnd'"/>
      <xsl:value-of select="'}}; '"/>
    </xsl:element>
  </xsl:template>

  <xsl:variable name="ImageLoad">
    <xsl:element name="Code">
      <xsl:value-of select="concat('var NumImages = ', count(//IATDisplayItem), ';')"/>
    </xsl:element>
    <xsl:element name="Code">var ImageLoadCtr = 0;</xsl:element>
    <xsl:element name="Code">var LoadingImagesElement = document.createElement("h3");</xsl:element>
    <xsl:element name="Code">var LoadingImagesText = document.createTextNode("Please Wait");</xsl:element>
    <xsl:element name="Code">LoadingImagesElement.appendChild(LoadingImagesText);</xsl:element>
    <xsl:element name="Code">var LoadingImagesProgressElement = document.createElement("h4");</xsl:element>
    <xsl:element name="Code">ImageLoadStatusTextElement = document.createTextNode("");</xsl:element>
    <xsl:element name="Code">ImageLoadStatusTextElement.nodeValue = "Loading image #1 of " + NumImages.toString();</xsl:element>
    <xsl:element name="Code">LoadingImagesProgressElement.appendChild(ImageLoadStatusTextElement);</xsl:element>
    <xsl:element name="Code">Display.divTag.appendChild(LoadingImagesElement);</xsl:element>
    <xsl:element name="Code">Display.divTag.appendChild(LoadingImagesProgressElement);</xsl:element>
    <xsl:element name="Code">var ImageLoadCtr = 0;</xsl:element>
    <xsl:element name="Code">var AllImagesLoaded = false;</xsl:element>
    <xsl:for-each select="//IATDisplayItem">
      <xsl:element name="Code">
        <xsl:value-of select="'var img', /ID, ' = new Image();'" />
      </xsl:element>
      <xsl:element name="Code">EventUtil.addHandler(img, load, OnImageLoad);"</xsl:element>
      <xsl:element name="Code">
        <xsl:value-of select="concat('img.src = ', Filename, ';')" />
      </xsl:element>
    </xsl:for-each>
  </xsl:variable>

  <xsl:variable name="Classes">
    <xsl:element name="Class">
      <xsl:attribute name="ClassName" select="'IATDisplayItem'"/>
      <xsl:element name="Constructor">
        <xsl:element name="Params">
          <xsl:element name="Param">id</xsl:element>
          <xsl:element name="Param">src</xsl:element>
          <xsl:element name="Param">x</xsl:element>
          <xsl:element name="Param">y</xsl:element>
          <xsl:element name="Param">width</xsl:element>
          <xsl:element name="Param">height</xsl:element>
        </xsl:element>
        <xsl:variable name="constructorBodyElems">
          <xsl:element name="Code">this.id = id;</xsl:element>
          <xsl:element name="Code">
            <xsl:value-of select="concat('this.src = &quot;', ./ServerURL, ', ', ./ClientID, '/', ./IATName, '/ + src;')"/>
          </xsl:element>
          <xsl:element name="Code">this.x = x;</xsl:element>
          <xsl:element name="Code">this.y = y;</xsl:element>
          <xsl:element name="Code">this.width = width;</xsl:element>
          <xsl:element name="Code">this.height = height;</xsl:element>
          <xsl:element name="Code">this.img = new Image();</xsl:element>
          <xsl:element name="Code">this.imgTag = document.createElement("img");</xsl:element>
          <xsl:element name="Code">this.imgTag.appendChild(this.img);</xsl:element>
          <xsl:element name="Code">this.imgTag.id = "IATDisplayItem" + id.toString();</xsl:element>
        </xsl:variable>
        <xsl:element name="ConstructorBody">
          <xsl:for-each select="$constructorBodyElems/Code">
            <xsl:element name="Code">
              <xsl:attribute name="LineNum" select="position"/>
              <xsl:value-of select="."/>
            </xsl:element>
          </xsl:for-each>
        </xsl:element>
      </xsl:element>

      <xsl:element name="PrototypeChain">
        <xsl:element name="Function">
          <xsl:attribute name="FunctionName" select="'Load'"/>
          <xsl:element name="Params"/>
          <xsl:variable name="functionBodyElems">
            <xsl:element name="Code">EventUtil.addHandler(this.img, "load", OnImageLoad);</xsl:element>
            <xsl:element name="Code">this.img.src = this.src;</xsl:element>
          </xsl:variable>
          <xsl:element name="FunctionBody">
            <xsl:for-each select="$functionBodyElems/Code">
              <xsl:element name="Code">
                <xsl:attribute name="LineNum" select="position()"/>
                <xsl:value-of select="."/>
              </xsl:element>
            </xsl:for-each>
          </xsl:element>
        </xsl:element>

        <xsl:element name="Function">
          <xsl:attribute name="FunctionName" select="'Outline'"/>
          <xsl:element name="Params"/>
          <xsl:variable name="functionBodyElems">
            <xsl:element name="Code">this.imgTag.className = "outlinedDI";</xsl:element>
          </xsl:variable>
          <xsl:element name="FunctionBody">
            <xsl:for-each select="$functionBodyElems/Code">
              <xsl:element name="Code">
                <xsl:attribute name="LineNum" select="position()"/>
                <xsl:value-of select="."/>
              </xsl:element>
            </xsl:for-each>
          </xsl:element>
        </xsl:element>

        <xsl:element name="Function">
          <xsl:attribute name="FunctionName" select="'Display'"/>
          <xsl:element name="Params">
            <xsl:element name="Param">parentNode</xsl:element>
          </xsl:element>
          <xsl:variable name="functionBodyElems">
            <xsl:element name="Code">this.imgTag.src = this.src;</xsl:element>
            <xsl:element name="Code">parentNode.appendChild(this.imgTag);</xsl:element>
          </xsl:variable>
          <xsl:element name="FunctionBody">
            <xsl:for-each select="$functionBodyElems/Code">
              <xsl:element name="Code">
                <xsl:attribute name="LineNum" select="position()"/>
                <xsl:value-of select="."/>
              </xsl:element>
            </xsl:for-each>
          </xsl:element>
        </xsl:element>

        <xsl:element name="Function">
          <xsl:attribute name="FunctionName" select="'Hide'"/>
          <xsl:element name="Params"/>
          <xsl:variable name="functionBodyElems">
            <xsl:element name="Code">if (this.imgTag.parentNode) {</xsl:element>
            <xsl:element name="Code">this.imgTag.parentNode.removeChild(this.imgTag);</xsl:element>
            <xsl:element name="Code">}</xsl:element>
            <xsl:element name="Code">this.imgTag.className = "";</xsl:element>
          </xsl:variable>
          <xsl:element name="FunctionBody">
            <xsl:for-each select="$functionBodyElems/Code">
              <xsl:element name="Code">
                <xsl:attribute name="LineNum" select="position()"/>
                <xsl:value-of select="."/>
              </xsl:element>
            </xsl:for-each>
          </xsl:element>
        </xsl:element>
      </xsl:element>
    </xsl:element>


    <xsl:element name="Class">
      <xsl:attribute name="ClassName" select="'IATDisplay'"/>
      <xsl:element name="Constructor">
        <xsl:element name="Params"/>
        <xsl:variable name="constructorBodyElems">
          <xsl:element name="Code">
            <xsl:value-of select="concat('this.interiorWidth = ', //IATLayout/InteriorWidth, ';')"/>
          </xsl:element>
          <xsl:element name="Code">
            <xsl:value-of select="concat('this.interiorHeight = ', //IATLayout/InteriorHeight, ';')"/>
          </xsl:element>
          <xsl:element name="Code">
            <xsl:value-of select="concat('this.leftResponseKeyCodeUpper = ', ./LeftResponseASCIIKeyCodeUpper, ';')"/>
          </xsl:element>
          <xsl:element name="Code">
            <xsl:value-of select="concat('this.rightResponseKeyCodeUpper = ', ./RightResponseASCIIKeyCodeUpper, ';')"/>
          </xsl:element>
          <xsl:element name="Code">
            <xsl:value-of select="concat('this.leftResponseKeyCodeLower = ', ./LeftResponseASCIIKeyCodeLower, ';')"/>
          </xsl:element>
          <xsl:element name="Code">
            <xsl:value-of select="concat('this.rightResponseKeyCodeLower = ', ./RightResponseASCIIKeyCodeLower, ';')"/>
          </xsl:element>
          <xsl:element name="Code">this.divTag  = document.getElementById("IATDisplayDiv");</xsl:element>
          <xsl:element name="Code">this.displayItems = new Array();</xsl:element>
        </xsl:variable>
        <xsl:element name="ConstructorBody">
          <xsl:for-each select="$constructorBodyElems/Code">
            <xsl:element name="Code">
              <xsl:attribute name="LineNum" select="position()"/>
              <xsl:value-of select="."/>
            </xsl:element>
          </xsl:for-each>
        </xsl:element>
      </xsl:element>

      <xsl:element name="PrototypeChain">
        <xsl:element name="Function">
          <xsl:attribute name="FunctionName" select="'AddDisplayItem'"/>
          <xsl:element name="Params">
            <xsl:element name="Param">di</xsl:element>
          </xsl:element>
          <xsl:variable name="functionBodyElems">
            <xsl:element name="Code">this.displayItems[this.displayItems.length] = di;</xsl:element>
            <xsl:element name="Code">di.Display(this.divTag);</xsl:element>
          </xsl:variable>
          <xsl:element name="FunctionBody">
            <xsl:for-each select="$functionBodyElems/Code">
              <xsl:element name="Code">
                <xsl:attribute name="LineNum" select="position()"/>
                <xsl:value-of select="."/>
              </xsl:element>
            </xsl:for-each>
          </xsl:element>
        </xsl:element>

        <xsl:element name="Function">
          <xsl:attribute name="FunctionName" select="'RemoveDisplayItem'"/>
          <xsl:element name="Params">
            <xsl:element name="Param">di</xsl:element>
          </xsl:element>
          <xsl:variable name="functionBodyElems">
            <xsl:element name="Code">for (var ctr = 0; ctr &lt; this.displayItems.length; ctr++) {</xsl:element>
            <xsl:element name="Code">if (this.displayItems[ctr].id == di.id) {</xsl:element>
            <xsl:element name="Code">this.displayItems[ctr].Hide();</xsl:element>
            <xsl:element name="Code">this.displayItems.splice(ctr, 1);</xsl:element>
            <xsl:element name="Code">}</xsl:element>
            <xsl:element name="Code">}</xsl:element>
          </xsl:variable>
          <xsl:element name="FunctionBody">
            <xsl:for-each select="$functionBodyElems/Code">
              <xsl:element name="Code">
                <xsl:attribute name="LineNum" select="position()"/>
                <xsl:value-of select="."/>
              </xsl:element>
            </xsl:for-each>
          </xsl:element>
        </xsl:element>

        <xsl:element name="Function">
          <xsl:attribute name="FunctionName" select="'Clear'"/>
          <xsl:element name="Params"/>
          <xsl:variable name="functionBodyElems">
            <xsl:element name="Code">for (var ctr = 0; ctr &lt; this.displayItems.length; ctr++)</xsl:element>
            <xsl:element name="Code">this.displayItems[ctr].Hide();</xsl:element>
            <xsl:element name="Code">this.displayItems.splice(0, this.displayItems.length);</xsl:element>
          </xsl:variable>
          <xsl:element name="FunctionBody">
            <xsl:for-each select="$functionBodyElems/Code">
              <xsl:element name="Code">
                <xsl:attribute name="LineNum" select="position()"/>
                <xsl:value-of select="."/>
              </xsl:element>
            </xsl:for-each>
          </xsl:element>
        </xsl:element>

        <xsl:element name="Function">
          <xsl:attribute name="FunctionName" select="'StartTimer'"/>
          <xsl:element name="Params"/>
          <xsl:variable name="functionBodyElems">
            <xsl:element name="Code">this.startTime = (new Date()).getTime();</xsl:element>
          </xsl:variable>
          <xsl:element name="FunctionBody">
            <xsl:for-each select="$functionBodyElems/Code">
              <xsl:element name="Code">
                <xsl:attribute name="LineNum" select="position()"/>
                <xsl:value-of select="."/>
              </xsl:element>
            </xsl:for-each>
          </xsl:element>
        </xsl:element>
      </xsl:element>
    </xsl:element>

    <xsl:element name="Class">
      <xsl:attribute name="ClassName" select="'IATEvent'"/>
      <xsl:element name="Constructor">
        <xsl:element name="Params">
          <xsl:element name="Param">id</xsl:element>
          <xsl:element name="Param">handler</xsl:element>
        </xsl:element>
        <xsl:variable name="constructorBodyElems">
          <xsl:element name="Code">this.id = id;</xsl:element>
          <xsl:element name="Code">this.handler = handler;</xsl:element>
        </xsl:variable>
        <xsl:element name="ConstructorBody">
          <xsl:for-each select="$constructorBodyElems/Code">
            <xsl:element name="Code">
              <xsl:attribute name="LineNum" select="position()"/>
              <xsl:value-of select="."/>
            </xsl:element>
          </xsl:for-each>
        </xsl:element>
      </xsl:element>

      <xsl:element name="PrototypeChain">
        <xsl:element name="Function">
          <xsl:attribute name="FunctionName" select="'Execute'"/>
          <xsl:element name="Params"/>
          <xsl:variable name="functionBodyElems">
            <xsl:element name="Code">if (this.handler == null)</xsl:element>
            <xsl:element name="Code">EventList[++EventCtr].Execute();</xsl:element>
            <xsl:element name="Code">else {</xsl:element>
            <xsl:element name="Code">currentHandler = this.handler;</xsl:element>
            <xsl:element name="Code">EventUtil.addHandler(Display.divTag, "keypress", this.handler);</xsl:element>
            <xsl:element name="Code">}</xsl:element>
          </xsl:variable>
          <xsl:element name="ConstructorBody">
            <xsl:for-each select="$functionBodyElems/Code">
              <xsl:element name="Code">
                <xsl:attribute name="LineNum" select="position()"/>
                <xsl:value-of select="."/>
              </xsl:element>
            </xsl:for-each>
          </xsl:element>
        </xsl:element>
      </xsl:element>
    </xsl:element>

    <xsl:element name="Class">
      <xsl:attribute name="ClassName" select="'IATSubmitEvent'"/>
      <xsl:element name="Constructor">
        <xsl:element name="Params">
          <xsl:element name="Param">id</xsl:element>
        </xsl:element>
        <xsl:variable name="constructorBodyElems">
          <xsl:element name="Code">IATEvent.call(this, id, null);</xsl:element>
        </xsl:variable>
        <xsl:element name="ConstructorBody">
          <xsl:for-each select="$constructorBodyElems">
            <xsl:element name="Code">
              <xsl:attribute name="LineNum" select="position()"/>
              <xsl:value-of select="."/>
            </xsl:element>
          </xsl:for-each>
        </xsl:element>
      </xsl:element>

      <xsl:element name="PrototypeChain">
        <xsl:element name="Function">
          <xsl:attribute name="FunctionName" select="'Execute'"/>
          <xsl:element name="Params"/>
          <xsl:variable name="functionBodyElems">
            <xsl:element name="Code">var numItemsInput = document.createElement("input");</xsl:element>
            <xsl:element name="Code">numItemsInput.name = "NumItems";</xsl:element>
            <xsl:element name="Code">numItemsInput.type = "hidden";</xsl:element>
            <xsl:element name="Code">numItemsInput.value = currentItemNum.toString();</xsl:element>
            <xsl:element name="Code">Display.divTag.appendChild(numItemsInput);</xsl:element>
            <xsl:element name="Code">var form = document.getElementById("IATForm");</xsl:element>
            <xsl:element name="Code">form.submit();</xsl:element>
          </xsl:variable>
          <xsl:element name="FunctionBody">
            <xsl:for-each select="$functionBodyElems/Code">
              <xsl:element name="Code">
                <xsl:attribute name="LineNum" select="position()"/>
                <xsl:value-of select="."/>
              </xsl:element>
            </xsl:for-each>
          </xsl:element>
        </xsl:element>
      </xsl:element>
    </xsl:element>

    <xsl:element name="Class">
      <xsl:attribute name="ClassName" select="'IATItem'"/>
      <xsl:element name="Constructor">
        <xsl:element name="Params">
          <xsl:element name="Param">id</xsl:element>
          <xsl:element name="Param">stimulus</xsl:element>
          <xsl:element name="Param">itemNum</xsl:element>
          <xsl:element name="Param">keyedDir</xsl:element>
        </xsl:element>
        <xsl:variable name="constructorBodyElems">
          <xsl:element name="Code">if (keyedDir == "Left") {</xsl:element>
          <xsl:element name="Code">IATEvent.call(this, id, function(event) {</xsl:element>
          <xsl:element name="Code">event = EventUtil.getEvent(event);</xsl:element>
          <xsl:element name="Code">var keyCode = EventUtil.getCharCode(event);</xsl:element>
          <xsl:element name="Code">if ((keyCode == Display.leftResponseKeyCodeUpper) || (keyCode == Display.leftResponseKeyCodeLower)) {</xsl:element>
          <xsl:element name="Code">if (!inPracticeBlock) {</xsl:element>
          <xsl:element name="Code">var latencyItemName = document.createElement("input");</xsl:element>
          <xsl:element name="Code">latencyItemName.name = "ItemNum" + currentItemNum.toString();</xsl:element>
          <xsl:element name="Code">latencyItemName.value = currentItemID.toString();</xsl:element>
          <xsl:element name="Code">latencyItemName.type = "hidden";</xsl:element>
          <xsl:element name="Code">Display.divTag.appendChild(latencyItemName);</xsl:element>
          <xsl:element name="Code">var latency = (new Date()).getTime() - Display.startTime;</xsl:element>
          <xsl:element name="Code">var latencyOutput = document.createElement("input");</xsl:element>
          <xsl:element name="Code">latencyOutput.name = "Item" + currentItemNum.toString();</xsl:element>
          <xsl:element name="Code">currentItemNum++;</xsl:element>
          <xsl:element name="Code">latencyOutput.type = "hidden";</xsl:element>
          <xsl:element name="Code">latencyOutput.value = latency.toString();</xsl:element>
          <xsl:element name="Code">Display.divTag.appendChild(latencyOutput);</xsl:element>
          <xsl:element name="Code">}</xsl:element>
          <xsl:element name="Code">if (isErrorMarked)</xsl:element>
          <xsl:element name="Code">Display.RemoveDisplayItem(ErrorMark);</xsl:element>
          <xsl:element name="Code">isErrorMarked = false;</xsl:element>
          <xsl:element name="Code">Display.RemoveDisplayItem(currentStimulus);</xsl:element>
          <xsl:element name="Code">EventUtil.removeHandler(Display.divTag, "keypress", currentHandler);</xsl:element>
          <xsl:element name="Code">EventList[++EventCtr].Execute();</xsl:element>
          <xsl:element name="Code">} else if ((keyCode == Display.rightResponseKeyCodeUpper) || (keyCode == Display.rightResponseKeyCodeLower)) {</xsl:element>
          <xsl:element name="Code">if (!isErrorMarked) {</xsl:element>
          <xsl:element name="Code">Display.AddDisplayItem(ErrorMark);</xsl:element>
          <xsl:element name="Code">isErrorMarked = true;</xsl:element>
          <xsl:element name="Code">}</xsl:element>
          <xsl:element name="Code">}</xsl:element>
          <xsl:element name="Code">});</xsl:element>
          <xsl:element name="Code">} else {</xsl:element>
          <xsl:element name="Code">IATEvent.call(this, id, function(event) {</xsl:element>
          <xsl:element name="Code">event = EventUtil.getEvent(event);</xsl:element>
          <xsl:element name="Code">var keyCode = EventUtil.getCharCode(event);</xsl:element>
          <xsl:element name="Code">if ((keyCode == Display.rightResponseKeyCodeUpper) || (keyCode == Display.rightResponseKeyCodeLower)) {</xsl:element>
          <xsl:element name="Code">if (!inPracticeBlock) {</xsl:element>
          <xsl:element name="Code">var latencyItemName = document.createElement("input");</xsl:element>
          <xsl:element name="Code">latencyItemName.name = "ItemNum" + currentItemNum.toString();</xsl:element>
          <xsl:element name="Code">latencyItemName.value = currentItemID.toString();</xsl:element>
          <xsl:element name="Code">latencyItemName.type = "hidden";</xsl:element>
          <xsl:element name="Code">Display.divTag.appendChild(latencyItemName);</xsl:element>
          <xsl:element name="Code">var latency = (new Date()).getTime() - Display.startTime;</xsl:element>
          <xsl:element name="Code">var latencyOutput = document.createElement("input");</xsl:element>
          <xsl:element name="Code">latencyOutput.name = "Item" + currentItemNum.toString();</xsl:element>
          <xsl:element name="Code">currentItemNum++;</xsl:element>
          <xsl:element name="Code">latencyOutput.type = "hidden";</xsl:element>
          <xsl:element name="Code">latencyOutput.value = latency.toString();</xsl:element>
          <xsl:element name="Code">Display.divTag.appendChild(latencyOutput);</xsl:element>
          <xsl:element name="Code">}</xsl:element>
          <xsl:element name="Code">if (isErrorMarked)</xsl:element>
          <xsl:element name="Code">Display.RemoveDisplayItem(ErrorMark);</xsl:element>
          <xsl:element name="Code">isErrorMarked = false;</xsl:element>
          <xsl:element name="Code">Display.RemoveDisplayItem(currentStimulus);</xsl:element>
          <xsl:element name="Code">EventUtil.removeHandler(Display.divTag, "keypress", currentHandler);</xsl:element>
          <xsl:element name="Code">EventList[++EventCtr].Execute();</xsl:element>
          <xsl:element name="Code">} else if ((keyCode == Display.leftResponseKeyCodeUpper) || (keyCode == Display.leftResponseKeyCodeLower)) {</xsl:element>
          <xsl:element name="Code">if (!isErrorMarked) {</xsl:element>
          <xsl:element name="Code">Display.AddDisplayItem(ErrorMark);</xsl:element>
          <xsl:element name="Code">isErrorMarked = true;</xsl:element>
          <xsl:element name="Code">}</xsl:element>
          <xsl:element name="Code">}</xsl:element>
          <xsl:element name="Code">});</xsl:element>
          <xsl:element name="Code">}</xsl:element>
          <xsl:element name="Code">this.isErrorMarked = false;</xsl:element>
          <xsl:element name="Code">this.stimulus = stimulus;</xsl:element>
          <xsl:element name="Code">this.itemNum = itemNum;</xsl:element>
        </xsl:variable>
        <xsl:element name="ConstructorBody">
          <xsl:for-each select="$constructorBodyElems/Code">
            <xsl:element name="Code">
              <xsl:attribute name="LineNum" select="position()"/>
              <xsl:value-of select="."/>
            </xsl:element>
          </xsl:for-each>
        </xsl:element>
      </xsl:element>

      <xsl:element name="PrototypeChain">
        <xsl:element name="Function">
          <xsl:attribute name="FunctionName" select="'Execute'"/>
          <xsl:element name="Params"/>
          <xsl:variable name="functionBodyElems">
            <xsl:element name="Code">currentItemKeyedDir = this.keyedDir;</xsl:element>
            <xsl:element name="Code">currentStimulus = this.stimulus;</xsl:element>
            <xsl:element name="Code">currentItemID = this.itemNum;</xsl:element>
            <xsl:element name="Code">Display.AddDisplayItem(this.stimulus);</xsl:element>
            <xsl:element name="Code">Display.StartTimer();</xsl:element>
            <xsl:element name="Code">while ((new Date()).getTime() - 100 &lt; Display.startTime);</xsl:element>
            <xsl:element name="Code">IATEvent.prototype.Execute.call(this);</xsl:element>
          </xsl:variable>
          <xsl:element name="FunctionBody">
            <xsl:for-each select="$functionBodyElems/Code">
              <xsl:element name="Code">
                <xsl:attribute name="LineNum" select="position()"/>
                <xsl:value-of select="."/>
              </xsl:element>
            </xsl:for-each>
          </xsl:element>
        </xsl:element>
      </xsl:element>
    </xsl:element>

    <xsl:element name="Class">
      <xsl:attribute name="ClassNae" select="'IATBeginBlock'"/>
      <xsl:element name="Constructor">
        <xsl:element name="Params">
          <xsl:element name="Param">id</xsl:element>
          <xsl:element name="Param">isPracticeBlock</xsl:element>
          <xsl:element name="Param">leftDisplayItem</xsl:element>
          <xsl:element name="Param">rightDisplayItem</xsl:element>
          <xsl:element name="Param">instructionsDisplayItem</xsl:element>
        </xsl:element>
        <xsl:variable name="constructorBodyElems">
          <xsl:element name="Code">IATEvent.call(this, id, null);</xsl:element>
          <xsl:element name="Code">this.isPracticeBlock = isPracticeBlock;</xsl:element>
          <xsl:element name="Code">this.leftDisplayItem = leftDisplayItem;</xsl:element>
          <xsl:element name="Code">this.rightDisplayItem = rightDisplayItem;</xsl:element>
          <xsl:element name="Code">this.instructionsDisplayItem = instructionsDisplayItem;</xsl:element>
        </xsl:variable>
        <xsl:element name="ConstructorBody">
          <xsl:for-each select="$constructorBodyElems/Code">
            <xsl:element name="Code">
              <xsl:attribute name="LineNum" select="position()"/>
              <xsl:value-of select="."/>
            </xsl:element>
          </xsl:for-each>
        </xsl:element>
      </xsl:element>

      <xsl:element name="PrototypeChain">
        <xsl:element name="Function">
          <xsl:attribute name="FunctionName" select="'Execute'"/>
          <xsl:element name="Params"/>
          <xsl:variable name="functionBodyElems">
            <xsl:element name="Code">inPracticeBlock = this.isPracticeBlock;</xsl:element>
            <xsl:element name="Code">Display.AddDisplayItem(this.leftDisplayItem);</xsl:element>
            <xsl:element name="Code">Display.AddDisplayItem(this.rightDisplayItem);</xsl:element>
            <xsl:element name="Code">Display.AddDisplayItem(this.instructionsDisplayItem);</xsl:element>
            <xsl:element name="Code">IATEvent.prototype.Execute.call(this);</xsl:element>
          </xsl:variable>
          <xsl:element name="FunctionBody">
            <xsl:for-each select="$functionBodyElems/Code">
              <xsl:element name="Code">
                <xsl:attribute name="LineNum" select="position()"/>
                <xsl:value-of select="."/>
              </xsl:element>
            </xsl:for-each>
          </xsl:element>
        </xsl:element>
      </xsl:element>
    </xsl:element>

    <xsl:element name="Class">
      <xsl:attribute name="ClassName" select="'IATEndBlock'"/>
      <xsl:element name="Constructor">
        <xsl:element name="Params">
          <xsl:element name="Param">id</xsl:element>
        </xsl:element>
        <xsl:variable name="constructorBodyElems">
          <xsl:element name="Code">IATEvent.call(this, id, null);</xsl:element>
        </xsl:variable>
        <xsl:element name="ConstructorBody">
          <xsl:for-each select="$constructorBodyElems/Code">
            <xsl:element name="Code">
              <xsl:attribute name="LineNum" select="position()"/>
              <xsl:value-of select="."/>
            </xsl:element>
          </xsl:for-each>
        </xsl:element>
      </xsl:element>

      <xsl:element name="PrototypeChain">
        <xsl:element name="Function">
          <xsl:attribute name="FunctionName" select="'Execute'"/>
          <xsl:element name="Params"/>
          <xsl:variable name="functionBodyElems">
            <xsl:element name="Code">inPracticeBlock = false;</xsl:element>
            <xsl:element name="Code">Display.Clear();</xsl:element>
            <xsl:element name="Code">IATEvent.prototype.Execute.call(this);</xsl:element>
          </xsl:variable>
          <xsl:element name="FunctionBody">
            <xsl:for-each select="$functionBodyElems/Code">
              <xsl:element name="Code">
                <xsl:attribute name="LineNum" select="position()"/>
                <xsl:value-of select="."/>
              </xsl:element>
            </xsl:for-each>
          </xsl:element>
        </xsl:element>
      </xsl:element>
    </xsl:element>

    <xsl:element name="Class">
      <xsl:attribute name="ClassName" select="'IATInstructionScreen'"/>
      <xsl:element name="Constructor">
        <xsl:element name="Params">
          <xsl:element name="Param">id</xsl:element>
          <xsl:element name="Param">continueChar</xsl:element>
          <xsl:element name="Param">continueInstructionsDI</xsl:element>
        </xsl:element>
        <xsl:variable name="constructorBodyElems">
          <xsl:element name="Code">IATEvent.call(this, id, function(event) {</xsl:element>
          <xsl:element name="Code">event = EventUtil.getEvent(event);</xsl:element>
          <xsl:element name="Code">var keyCode = EventUtil.getCharCode(event);</xsl:element>
          <xsl:element name="Code">if (keyCode == currentContinueKeyCode) {</xsl:element>
          <xsl:element name="Code">Display.Clear();</xsl:element>
          <xsl:element name="Code">EventUtil.removeHandler(Display.divTag, "keypress", currentHandler);</xsl:element>
          <xsl:element name="Code">IATEvent.prototype.Execute.call(this);</xsl:element>
          <xsl:element name="Code">}</xsl:element>
          <xsl:element name="Code">});</xsl:element>
          <xsl:element name="Code">this.continueChar = continueChar;</xsl:element>
          <xsl:element name="Code">this.continueInstructionsDI = continueInstructionsDI;</xsl:element>
        </xsl:variable>
        <xsl:element name="ConstructorBody">
          <xsl:for-each select="$constructorBodyElems/Code">
            <xsl:element name="Code">
              <xsl:attribute name="LineNum" select="position()"/>
              <xsl:value-of select="."/>
            </xsl:element>
          </xsl:for-each>
        </xsl:element>
      </xsl:element>

      <xsl:element name="PrototypeChain">
        <xsl:element name="Function">
          <xsl:attribute name="FunctionName" select="'Execute'"/>
          <xsl:variable name="functionBodyElems">
            <xsl:element name="Code">Display.AddDisplayItem(this.continueInstructionsDI);</xsl:element>
            <xsl:element name="Code">currentContinueKeyCode = this.continueChar;</xsl:element>
            <xsl:element name="Code">IATEvent.prototype.Execute.call(this);</xsl:element>
          </xsl:variable>
          <xsl:element name="FunctionBody">
            <xsl:for-each select="$functionBodyElems/Code">
              <xsl:element name="Code">
                <xsl:attribute name="LineNum" select="position()"/>
                <xsl:value-of select="."/>
              </xsl:element>
            </xsl:for-each>
          </xsl:element>
        </xsl:element>
      </xsl:element>
    </xsl:element>

    <xsl:element name="Class">
      <xsl:attribute name="ClassName" select="'IATTextInstructionsScreen'"/>
      <xsl:element name="Constructor">
        <xsl:element name="Params">
          <xsl:element name="Param">id</xsl:element>
          <xsl:element name="Param">continueChar</xsl:element>
          <xsl:element name="Param">continueInstructionsDI</xsl:element>
          <xsl:element name="Param">textInstructionsDI</xsl:element>
        </xsl:element>
        <xsl:variable name="constructorBodyElems">
          <xsl:element name="Code">IATInstructionScreen.call(this, id, continueChar, continueInstructionsDI);</xsl:element>
          <xsl:element name="Code">this.textInstructionsDI = textInstructionsDI;</xsl:element>
        </xsl:variable>
        <xsl:element name="ConstructorBody">
          <xsl:for-each select="$constructorBodyElems/Code">
            <xsl:element name="Code">
              <xsl:attribute name="LineNum" select="position()"/>
              <xsl:value-of select="."/>
            </xsl:element>
          </xsl:for-each>
        </xsl:element>
      </xsl:element>

      <xsl:element name="PrototypeChain">
        <xsl:element name="Function">
          <xsl:attribute name="FunctionName" select="'Execute'"/>
          <xsl:element name="Params"/>
          <xsl:variable name="functionBodyElems">
            <xsl:element name="Code">Display.AddDisplayItem(this.textInstructionsDI);</xsl:element>
            <xsl:element name="Code">IATInstructionScreen.prototype.Execute.call(this);</xsl:element>
          </xsl:variable>
          <xsl:element name="FunctionBody">
            <xsl:for-each select="$functionBodyElems/Code">
              <xsl:element name="Code">
                <xsl:attribute name="LineNum" select="position()"/>
                <xsl:value-of select="."/>
              </xsl:element>
            </xsl:for-each>
          </xsl:element>
        </xsl:element>
      </xsl:element>
    </xsl:element>

    <xsl:element name="Class">
      <xsl:attribute name="ClassName" select="'IATMockItemInstructionScreen'"/>
      <xsl:element name="Constructor">
        <xsl:element name="Params">
          <xsl:element name="Param">id</xsl:element>
          <xsl:element name="Param">continueChar</xsl:element>
          <xsl:element name="Param">continueInstructionsDI</xsl:element>
          <xsl:element name="Param">leftResponseDI</xsl:element>
          <xsl:element name="Param">rightResponseDI</xsl:element>
          <xsl:element name="Param">stimulusDI</xsl:element>
          <xsl:element name="Param">instructionsDI</xsl:element>
          <xsl:element name="Param">errorMarked</xsl:element>
          <xsl:element name="Param">outlineLeftResponse</xsl:element>
          <xsl:element name="Param">outlineRightResponse</xsl:element>
        </xsl:element>
        <xsl:variable name="constructorBodyElems">
          <xsl:element name="Code">IATInstructionScreen.call(this, id, continueChar, continueInstructionsDI);</xsl:element>
          <xsl:element name="Code">this.leftResponseDI = leftResponseDI;</xsl:element>
          <xsl:element name="Code">this.rightResponseDI = rightResponseDI;</xsl:element>
          <xsl:element name="Code">this.stimulusDI = stimulusDI;</xsl:element>
          <xsl:element name="Code">this.instructionsDI = instructionsDI;</xsl:element>
          <xsl:element name="Code">this.errorMarked = errorMarked;</xsl:element>
          <xsl:element name="Code">this.outlineLeftResponse = outlineLeftResponse;</xsl:element>
          <xsl:element name="Code">this.outlineRightResponse = outlineRightResponse;</xsl:element>
        </xsl:variable>
        <xsl:element name="ConstructorBody">
          <xsl:for-each select="constructorBodyElems/Code">
            <xsl:element name="Code">
              <xsl:attribute name="LineNum" select="position()"/>
              <xsl:value-of select="."/>
            </xsl:element>
          </xsl:for-each>
        </xsl:element>
      </xsl:element>

      <xsl:element name="PrototypeChain">
        <xsl:element name="Function">
          <xsl:attribute name="FunctionName" select="'Execute'"/>
          <xsl:element name="Params"/>
          <xsl:variable name="functionBodyElems">
            <xsl:element name="Code">if (this.outlineLeftResponse)</xsl:element>
            <xsl:element name="Code">this.leftResponseDI.Outline();</xsl:element>
            <xsl:element name="Code">Display.AddDisplayItem(this.leftResponseDI);</xsl:element>
            <xsl:element name="Code">if (this.outlineRightResponse)</xsl:element>
            <xsl:element name="Code">this.rightResponseDI.Outline();</xsl:element>
            <xsl:element name="Code">Display.AddDisplayItem(this.rightResponseDI);</xsl:element>
            <xsl:element name="Code">Display.AddDisplayItem(this.stimulusDI);</xsl:element>
            <xsl:element name="Code">Display.AddDisplayItem(this.instructionsDI);</xsl:element>
            <xsl:element name="Code">if (this.errorMarked)</xsl:element>
            <xsl:element name="Code">Display.AddDisplayItem(ErrorMark);</xsl:element>
            <xsl:element name="Code">IATInstructionScreen.prototype.Execute.call(this);</xsl:element>
          </xsl:variable>
          <xsl:element name="Code">
            <xsl:for-each select="$functionBodyElems/Code">
              <xsl:element name="Code">
                <xsl:attribute name="LineNum" select="position()"/>
                <xsl:value-of select="."/>
              </xsl:element>
            </xsl:for-each>
          </xsl:element>
        </xsl:element>
      </xsl:element>
    </xsl:element>

    <xsl:element name="Class">
      <xsl:attribute name="ClassName" select="'IATKeyedInstructionScreen'"/>
      <xsl:element name="Constructor">
        <xsl:element name="Params">
          <xsl:element name="Param">id</xsl:element>
          <xsl:element name="Param">continueChar</xsl:element>
          <xsl:element name="Param">continueInstructionsDI</xsl:element>
          <xsl:element name="Param">instructionsDI</xsl:element>
          <xsl:element name="Param">leftResponseDI</xsl:element>
          <xsl:element name="Param">rightResponseDI</xsl:element>
        </xsl:element>
        <xsl:variable name="constructorBodyElems">
          <xsl:element name="Code">IATInstructionScreen.call(this, id, continueChar, continueInstructionsDI);</xsl:element>
          <xsl:element name="Code">this.instructionsDI = instructionsDI;</xsl:element>
          <xsl:element name="Code">this.leftResponseDI = leftResponseDI;</xsl:element>
          <xsl:element name="Code">this.rightResponseDI = rightResponseDI;</xsl:element>
        </xsl:variable>
        <xsl:element name="ConstructorBody">
          <xsl:for-each select="$constructorBodyElems/Code">
            <xsl:element name="Code">
              <xsl:attribute name="LineNum" select="position()"/>
              <xsl:value-of select="."/>
            </xsl:element>
          </xsl:for-each>
        </xsl:element>
      </xsl:element>

      <xsl:element name="PrototypeChain">
        <xsl:element name="Function">
          <xsl:attribute name="FunctionName" select="'Execute'"/>
          <xsl:element name="Params"/>
          <xsl:variable name="functionBodyElems">
            <xsl:element name="Code">Display.AddDisplayItem(this.instructionsDI);</xsl:element>
            <xsl:element name="Code">Display.AddDisplayItem(this.leftResponseDI);</xsl:element>
            <xsl:element name="Code">Display.AddDisplayItem(this.rightResponseDI);</xsl:element>
            <xsl:element name="Code">IATInstructionScreen.prototype.Executecall(this);</xsl:element>
          </xsl:variable>
          <xsl:element name="FunctionBody">
            <xsl:for-each select="$functionBodyElems/Code">
              <xsl:element name="Code">
                <xsl:attribute name="LineNum" select="position()"/>
                <xsl:value-of select="."/>
              </xsl:element>
            </xsl:for-each>
          </xsl:element>
        </xsl:element>
      </xsl:element>
    </xsl:element>

    <xsl:element name="Class">
      <xsl:attribute name="ClassName" select="'IATBlock'"/>
      <xsl:element name="Constructor">
        <xsl:element name="Params">
          <xsl:element name="Param">blockNum</xsl:element>
          <xsl:element name="Param">numPresentations</xsl:element>
          <xsl:element name="Param">alternatedWith</xsl:element>
        </xsl:element>
        <xsl:variable name="constructorBodyElems">
          <xsl:element name="Code">this.blockNum = blockNum;</xsl:element>
          <xsl:element name="Code">this.numPresentations = numPresentations;</xsl:element>
          <xsl:element name="Code">this.alternatedWith = alternatedWith;</xsl:element>
          <xsl:element name="Code">this.BeginBlockEvent = null;</xsl:element>
          <xsl:element name="Code">this.EndBlockEvent = null;</xsl:element>
          <xsl:element name="Code">this.Items = new Array();</xsl:element>
        </xsl:variable>
        <xsl:element name="ConstructorBody">
          <xsl:for-each select="$constructorBodyElems/Code">
            <xsl:element name="Code">
              <xsl:attribute name="LineNum" select="position()"/>
              <xsl:value-of select="."/>
            </xsl:element>
          </xsl:for-each>
        </xsl:element>
      </xsl:element>

      <xsl:element name="PrototypeChain">
        <xsl:element name="Function">
          <xsl:attribute name="FunctionName" select="'AddItem'"/>
          <xsl:element name="Params">
            <xsl:element name="Param">item</xsl:element>
          </xsl:element>
          <xsl:variable name="functionBodyElems">
            <xsl:element name="Code">this.Items.push (item);</xsl:element>
          </xsl:variable>
          <xsl:element name="FunctionBody">
            <xsl:for-each select="$functionBodyElems/Code">
              <xsl:attribute name="LineNum" select="position()"/>
              <xsl:value-of select="."/>
            </xsl:for-each>
          </xsl:element>
        </xsl:element>

        <xsl:element name="Function">
          <xsl:attribute name="FunctionName" select="'GenerateContents'"/>
          <xsl:element name="Params">
            <xsl:element name="Param">randomization</xsl:element>
          </xsl:element>
          <xsl:variable name="functionBodyElems">
            <xsl:element name="Code">var result = new Array();</xsl:element>
            <xsl:element name="Code">result.push(this.BeginBlockEvent);</xsl:element>
            <xsl:element name="Code">var ctr;</xsl:element>
            <xsl:element name="Code">var currItemNdx, lastItemNdx = -1;</xsl:element>
            <xsl:element name="Code">if (randomization == "None") {</xsl:element>
            <xsl:element name="Code">for (ctr = 0; ctr &lt; Items.length; ctr++)</xsl:element>
            <xsl:element name="Code">result.push(this.Items[ctr]);</xsl:element>
            <xsl:element name="Code">} else if (randomization == "RandomOrder") {</xsl:element>
            <xsl:element name="Code">var tempItems = new Array();</xsl:element>
            <xsl:element name="Code">for (ctr = 0; ctr &lt; this.Items.length; ctr++)</xsl:element>
            <xsl:element name="Code">tempItems.push(this.Items[ctr]);</xsl:element>
            <xsl:element name="Code">for (ctr = 0; ctr &lt; this.Items.length; ctr++) {</xsl:element>
            <xsl:element name="Code">var ndx = Math.floor(Math.random() * tempItems.length);</xsl:element>
            <xsl:element name="Code">result.push(tempItems[ndx]);</xsl:element>
            <xsl:element name="Code">tempItems.splice(ndx, 1);</xsl:element>
            <xsl:element name="Code">}</xsl:element>
            <xsl:element name="Code">} else if (randomization == "SetNumberOfPresentations") {</xsl:element>
            <xsl:element name="Code">for (ctr = 0; ctr &lt; this.numPresentations; ctr++) {</xsl:element>
            <xsl:element name="Code">currItemNdx = Math.floor(Math.random() * this.Items.length);</xsl:element>
            <xsl:element name="Code">while (currItemNdx == lastItemNdx)</xsl:element>
            <xsl:element name="Code">currItemNdx = Math.floor(Math.random() * this.Items.length);</xsl:element>
            <xsl:element name="Code">result.push(this.Items[currItemNdx]);</xsl:element>
            <xsl:element name="Code">lastItemNdx = currItemNdx;</xsl:element>
            <xsl:element name="Code">}</xsl:element>
            <xsl:element name="Code">}</xsl:element>
            <xsl:element name="Code">result.push(this.EndBlockEvent);</xsl:element>
            <xsl:element name="Code">return result;</xsl:element>
          </xsl:variable>
          <xsl:element name="FunctionBody">
            <xsl:for-each select="$functionBodyElems/Code">
              <xsl:element name="Code">
                <xsl:attribute name="LineNum" select="position()"/>
                <xsl:value-of select="."/>
              </xsl:element>
            </xsl:for-each>
          </xsl:element>
        </xsl:element>
      </xsl:element>
    </xsl:element>

    <xsl:element name="Class">
      <xsl:attribute name="ClassName" select="'IATInstructionBlock'"/>
      <xsl:element name="Constructor">
        <xsl:element name="Params">
          <xsl:element name="Param">alternatedWith</xsl:element>
        </xsl:element>
        <xsl:variable name="constructorBodyElems">
          <xsl:element name="Code">this.alternatedWith = alternatedWith;</xsl:element>
          <xsl:element name="Code">this.screens = new Array();</xsl:element>
        </xsl:variable>
        <xsl:element name="ConstructorBody">
          <xsl:for-each select="ConstructoryBody/Code">
            <xsl:element name="Code">
              <xsl:attribute name="LineNum" select="position()"/>
              <xsl:value-of select="."/>
            </xsl:element>
          </xsl:for-each>
        </xsl:element>
      </xsl:element>

      <xsl:element name="PrototypeChain">
        <xsl:element name="Function">
          <xsl:attribute name="FunctionName" select="'AddScreen'"/>
          <xsl:element name="Params">
            <xsl:element name="Param">screen</xsl:element>
          </xsl:element>
          <xsl:variable name="functionBodyElems">
            <xsl:element name="Code">this.screens.push(screen);</xsl:element>
          </xsl:variable>
          <xsl:element name="FunctionBody">
            <xsl:for-each select="$functionBodyElems/Code">
              <xsl:element name="Code">
                <xsl:attribute name="LineNum" select="position()"/>
                <xsl:value-of select="."/>
              </xsl:element>
            </xsl:for-each>
          </xsl:element>
        </xsl:element>
      </xsl:element>
    </xsl:element>
  </xsl:variable>

  <xsl:variable name="Functions">
    <xsl:element name="Function">
      <xsl:attribute name="FunctionName" select="'OnImageLoad'"/>
      <xsl:element name="Params"/>
      <xsl:variable name="functionBodyElems">
        <xsl:element name="Code">ImageLoadCtr++;</xsl:element>
        <xsl:element name="Code">if (ImageLoadCtr == NumImages)</xsl:element>
        <xsl:element name="Code">OnImageLoadComplete();</xsl:element>
        <xsl:element name="Code">else</xsl:element>
        <xsl:element name="Code">ImageLoadStatusTextElement.nodeValue = "Loading image #" + (ImageLoadCtr + 1).toString() + " of " + NumImages.toString();</xsl:element>
      </xsl:variable>
      <xsl:element name="FunctionBody">
        <xsl:for-each select="$functionBodyElems/Code">
          <xsl:element name="Code">
            <xsl:attribute name="LineNum" select="position()"/>
            <xsl:value-of select="."/>
          </xsl:element>
        </xsl:for-each>
      </xsl:element>
    </xsl:element>

    <xsl:element name="Function">
      <xsl:attribute name="FunctionName" select="'BeginIAT'"/>
      <xsl:element name="Params"/>
      <xsl:variable name="functionBodyElems">
        <xsl:element name="Code">while (Display.divTag.firstChild)</xsl:element>
        <xsl:element name="Code">Display.divTag.removeChild(Display.divTag.firstChild);</xsl:element>
        <xsl:element name="Code">ClickToStartElement = document.createElement("h4");</xsl:element>
        <xsl:element name="Code">ClickToStartText = document.createTextNode("Click Here to Begin");</xsl:element>
        <xsl:element name="Code">ClickToStartElement.appendChild(ClickToStartText);</xsl:element>
        <xsl:element name="Code">Display.divTag.appendChild(ClickToStartElement);</xsl:element>
        <xsl:element name="Code">currentHandler = function() {</xsl:element>
        <xsl:element name="Code">Display.divTag.removeChild(ClickToStartElement);</xsl:element>
        <xsl:call-template name="GenerateEventInit">
          <xsl:with-param name="EventListNode" select="//IATEventList"/>
          <xsl:with-param name="randomization" select="//RandomizationType"/>
        </xsl:call-template>
        <xsl:element name="Code">EventUtil.removeHandler(Display.divTag, "click", currentHandler);</xsl:element>
        <xsl:element name="Code">EventList[EventCtr].Execute();</xsl:element>
        <xsl:element name="Code">}</xsl:element>
        <xsl:element name="Code">EventUtil.addHandler(Display.divTag, "click", currentHandler);</xsl:element>
        <xsl:element name="Code">Display.divTag.tabIndex = -1;</xsl:element>
        <xsl:element name="Code">Display.divTag.focus();</xsl:element>
        <xsl:element name="Code">var bodyTag = document.getElementById("bodyID");</xsl:element>
        <xsl:element name="Code">EventUtil.addHandler(bodyTag, "click", function() {</xsl:element>
        <xsl:element name="Code">Display.divTag.tabIndx = -1;</xsl:element>
        <xsl:element name="Code">Display.divTag.focus();</xsl:element>
        <xsl:element name="Code">});</xsl:element>
        <xsl:element name="Code">var containerDiv = document.getElementById("IATContainerDiv");</xsl:element>
        <xsl:element name="Code">EventUtil.addHandler(containerDiv, "click", function() {</xsl:element>
        <xsl:element name="Code">Display.divTag.tabIndex = -1;</xsl:element>
        <xsl:element name="Code">Display.divTag.focus();</xsl:element>
        <xsl:element name="Code">});</xsl:element>
      </xsl:variable>
      <xsl:element name="FunctionBody">
        <xsl:for-each select="$functionBodyElems/Code">
          <xsl:element name="Code">
            <xsl:attribute name="LineNum" select="position()"/>
            <xsl:value-of select="."/>
          </xsl:element>
        </xsl:for-each>
      </xsl:element>
    </xsl:element>

    <xsl:element name="Function">
      <xsl:attribute name="FunctionName" select="'BeginIATLoad'"/>
      <xsl:element name="Params"/>
      <xsl:variable name="functionBodyElems">
        <xsl:for-each select="//DisplayItemList/IATDisplayItem">
          <xsl:element name="Code">
            <xsl:value-of select="concat('DisplayItem', ./ID)"/> = new IATDisplayItem(<xsl:value-of select="concat(./ID, ', &quot;', ./Filename, '&quot;, ', ./X, ', ', ./Y, ', ', ./Width, ', ', ./Height, ');')"/>
          </xsl:element>
        </xsl:for-each>
        <xsl:variable name="paramList"
                      select="concat(//IATLayout/InteriorWidth, ', ', //IATLayout/InteriorHeight, ', ', //LeftResponseASCIIKeyCodeUpper, ', ', //LeftResponseASCIIKeyCodeLower, ', ', //RightResponseASCIIKeyCodeUpper, ', ', //RightResponseASCIIKeyCodeLower)"/>
        <xsl:element name="Code">
          <xsl:value-of select="concat('Display = new IATDisplay(', $paramList, ');')"/>
        </xsl:element>
        <xsl:element name="Code">
          <xsl:value-of select="concat('ErrorMark = DisplayItem', //ErrorMarkID, ');')"/>
        </xsl:element>
        <xsl:element name="Code">
          <xsl:value-of select="concat('ErrorMark.imgTag.id = DisplayItem', //ErrorMarkID, '.imgTag.id;')"/>
        </xsl:element>
      </xsl:variable>
      <xsl:element name="FunctionBody">
        <xsl:for-each select="$functionBodyElems/Code">
          <xsl:element name="Code">
            <xsl:attribute name="LineNum" select="position()"/>
            <xsl:value-of select="."/>
          </xsl:element>
        </xsl:for-each>
      </xsl:element>
    </xsl:element>

    <xsl:element name="Function">
      <xsl:attribute name="FunctionName" select="'GenerateEventList'" />
      <xsl:element name="Params" />
      <xsl:variable name="functionBodyElems">
        <xsl:apply-templates select="//IATEventList" />
      </xsl:variable>
      <xsl:element name="FunctionBody">
        <xsl:for-each select="$functionBodyElems/Code">
          <xsl:element name="Code">
            <xsl:attribute name="LineNum" select="position()" />
            <xsl:value-of select="." />
          </xsl:element>
        </xsl:for-each>
      </xsl:element>
    </xsl:element>

    <xsl:call-template name="GenerateProcessItemFunctions" />

  </xsl:variable>

  <xsl:template match="ConfigFile">
    <xsl:variable name="allCode">
      <xsl:call-template name="OutputEventUtil"/>
      <xsl:for-each select="$VariableDeclarations/Declarations/Declaration">
        <xsl:element name="CodeLine">
          <xsl:value-of select="."/>
        </xsl:element>
      </xsl:for-each>
      <xsl:for-each select="$Classes/Class">
        <xsl:call-template name="OutputClass">
          <xsl:with-param name="class" select="."/>
        </xsl:call-template>
      </xsl:for-each>
      <xsl:for-each select="$Functions/*">
        <xsl:call-template name="OutputFunction">
          <xsl:with-param name="function" select="."/>
        </xsl:call-template>
      </xsl:for-each>
    </xsl:variable>
    <xsl:element name="IAT">
      <xsl:attribute name="TestName" select="//IATName"/>
      <xsl:attribute name="ClientID" select="//ClientID"/>
      <xsl:attribute name="NumLines" select="count($allCode/CodeLine)" />
      <xsl:for-each select="$allCode/CodeLine">
        <xsl:variable name="currLine" select="."/>
        <xsl:element name="JSLine">
          <xsl:attribute name="Type" select="$currLine/@Type"/>
          <xsl:attribute name="Name" select="$currLine/@Name"/>
          <xsl:attribute name="Line" select="count(preceding-sibling::*) + 1"/>
          <xsl:value-of select="$currLine"/>
        </xsl:element>
      </xsl:for-each>
    </xsl:element>
  </xsl:template>



  <xsl:template name="MaskSpecifierArrayAppend">
    <xsl:param name="item"/>
    <xsl:if test="$item/KeyedDir eq 'DynamicLeft'">
      <xsl:element name="Code">
        <xsl:value-of select="concat('KeyedDirInput = document.getElementById(DynamicKey', $item/ID, ').value;')"/>
      </xsl:element>
      <xsl:element name="Code">if (KeyedDirInput == "True")</xsl:element>
      <xsl:element name="Code">KeyedDir = "Left";</xsl:element>
      <xsl:element name="Code">else</xsl:element>
      <xsl:element name="Code">KeyedDir = "Right";</xsl:element>
    </xsl:if>
    <xsl:if test="$item/KeyedDir eq 'DynamicRight'">
      <xsl:element name="Code">if (KeyedDirInput == "True")</xsl:element>
      <xsl:element name="Code">KeyedDir = "Right";</xsl:element>
      <xsl:element name="Code">else</xsl:element>
      <xsl:element name="Code">KeyedDir = "Left";</xsl:element>
    </xsl:if>
    <xsl:variable name="params"
                  select="concat(ItemNum, ', DisplayItem', $item/StimulusDisplayID, ', KeyedDir, ', OriginatingBlock)"/>
    <xsl:element name="Code">if (KeyedDirInput == "True")</xsl:element>
    <xsl:element name="Code">
      <xsl:value-of select="concat('MaskItemTrueArray.push(new Array(', $params, '));')"/>
    </xsl:element>
    <xsl:element name="Code">else</xsl:element>
    <xsl:element name="Code">
      <xsl:value-of select="concat('MaskItemFalseArray.push(new Array(', $params, '));')"/>
    </xsl:element>
  </xsl:template>

  <xsl:template name="MaskSpecifierArrayAppendRange">
    <xsl:param name="itemList"/>
    <xsl:element name="Code">KeyedDirArray = new Array();</xsl:element>
    <xsl:element name="Code">OriginatingBlockArray = new Array();</xsl:element>
    <xsl:element name="Code">StimulusIDArray = new Array();</xsl:element>
    <xsl:element name="Code">ItemNumArray = new Array();</xsl:element>
    <xsl:for-each select="$itemList">
      <xsl:element name="Code">
        <xsl:value-of select="concat('KeyedDirInput = document.getElementById(DynamicKey', ID, ').value;')"/>
      </xsl:element>
      <xsl:element name="Code">
        <xsl:value-of select="concat('ItemNumArray.push(', ItemNum, ');')"/>
      </xsl:element>
      <xsl:element name="Code">
        <xsl:value-of select="concat('OriginatingBlockArray.push(', OriginatingBlock, ');')"/>
      </xsl:element>
      <xsl:element name="Code">
        <xsl:value-of select="concat('StimulusIDArray.push(DisplayItem', StimulusDisplayID, ');')"/>
      </xsl:element>
      <xsl:if test="KeyedDir eq 'DynamicLeft'">
        <xsl:element name="Code">if (KeyedDirInput == "True")</xsl:element>
        <xsl:element name="Code">KeyedDir = "Left";</xsl:element>
        <xsl:element name="Code">else</xsl:element>
        <xsl:element name="Code">KeyedDir = "Right";</xsl:element>
      </xsl:if>
      <xsl:if test="KeyedDir eq 'DynamicRight'">
        <xsl:element name="Code">if (KeyedDirInput == "True")</xsl:element>
        <xsl:element name="Code">KeyedDir = "Right";</xsl:element>
        <xsl:element name="Code">else</xsl:element>
        <xsl:element name="Code">KeyedDir = "Left";</xsl:element>
      </xsl:if>
      <xsl:element name="Code">KeyedDirArray.push(KeyedDir);</xsl:element>
    </xsl:for-each>
    <xsl:element name="Code">randomNum = Math.floor(Math.random() * KeyedDirArray.length);</xsl:element>
    <xsl:element name="Code">KeyedDir = KeyedDirArray[randomNum];</xsl:element>
    <xsl:element name="Code">if (KeyedDirInput == "True")</xsl:element>
    <xsl:element name="Code">MaskItemTrueArray.push(new Array(ItemNumArray[randomNum], StimulusIDArray[randomNum], KeyedDir, OriginatingBlockArray[randomNum]));" /&gt;);</xsl:element>
    <xsl:element name="Code">else</xsl:element>
    <xsl:element name="Code">MaskItemFalseArray.push(new Array(ItemNumArray[randomNum], StimulusIDArray[randomNum], KeyedDir, OriginatingBlockArray[randomNum]));</xsl:element>
  </xsl:template>


  <xsl:template name="GenerateImageLoad">
    <xsl:param name="ImageListNode"/>
    <xsl:variable name="numImages" select="count($ImageListNode/IATDisplayItem)"/>
    <xsl:element name="Code">ImageLoadCtr = 0;</xsl:element>
    <xsl:element name="Code">var LoadingImagesElement = document.createElement("h3");</xsl:element>
    <xsl:element name="Code">var LoadingImagesText = document.createTextNode("Please Wait");</xsl:element>
    <xsl:element name="Code">LoadingImagesElement.appendChild(LoadingImagesText);</xsl:element>
    <xsl:element name="Code">var LoadingImagesProgressElement = document.createElement("h4");</xsl:element>
    <xsl:element name="Code">ImageLoadStatusTextElement = document.createTextNode("");</xsl:element>
    <xsl:element name="Code">ImageLoadStatusTextElement.nodeValue = "Loading image #1 of " + NumImages.toString();</xsl:element>
    <xsl:element name="Code">LoadingImagesProgressElement.appendChild(ImageLoadStatusTextElement);</xsl:element>
    <xsl:element name="Code">Display.divTag.appendChild(LoadingImagesElement);</xsl:element>
    <xsl:element name="Code">Display.divTag.appendChild(LoadingImagesProgressElement);</xsl:element>
    <xsl:for-each select="$ImageListNode/*">
      <xsl:element name="Code">
        <xsl:value-of select="concat('DisplayItem', ./ID, '.Load();')"/>
      </xsl:element>
    </xsl:for-each>
  </xsl:template>

  <xsl:template match="IATEventList">
    <xsl:for-each select="IATEvent">
      <xsl:choose>
        <xsl:when test="@EventType eq 'BeginIATBlock'">
          <xsl:variable name="blockLength" select="./NumItems" as="xs:integer"/>
          <xsl:element name="Code">
            <xsl:value-of select="concat('ItemBlocks.push(new IATBlock(ItemBlocks.length + 1, ', NumPresentations, ', ', ./AlternatedWith, '));')"/>
          </xsl:element>
          <xsl:element name="Code">
            <xsl:value-of select="concat('ItemBlocks[ItemBlocks.length - 1].BeginBlockEvent = new IATBeginBlock(', position(), ', ', lower-case(./PracticeBlock), ', DisplayItem', ./LeftResponseDisplayID, ', DisplayItem', ./RightResponseDisplayID, ', DisplayItem', ./InstructionsDisplayID, ');')"/>
          </xsl:element>
          <xsl:variable name="itemsInBlock" select="following::IATEvent[position() le $blockLength]"/>
          <xsl:call-template name="GenerateBlockLoad">
            <xsl:with-param name="items" select="$itemsInBlock"/>
            <xsl:with-param name="startPosition" select="xs:integer(position())" as="xs:integer"/>
          </xsl:call-template>
        </xsl:when>
        <xsl:when test="@EventType eq 'EndIATBlock'">
          <xsl:element name="Code">
            <xsl:value-of select="concat('ItemBlocks[ItemBlocks.length - 1].EndBlockEvent = new IATEndBlock(', position(), ');')" />
          </xsl:element>
        </xsl:when>
        <xsl:when test="@EventType eq 'BeginInstructionBlock'">
          <xsl:element name="Code">
            <xsl:value-of select="concat('InstructionBlocks.push(new IATInstructionBlocck(', AlternatedWith, ');')" />
          </xsl:element>
        </xsl:when>
        <xsl:when test="@EventType eq 'TextInstructionScreen'">
          <xsl:element name="Code">
            <xsl:value-of select="concat('InstructionBlocks[InstructionBlocks.length - 1].AddScreen(new IATTestInstructionScreen(', position(), ', ', ContinueASCIIKeyCode, ', DisplayItem', ContinueInstructionsDisplayID, ', DisplayItem', InstructionsDisplayID, '));')" />
          </xsl:element>
        </xsl:when>
        <xsl:when test="@EventType eq 'KeyedInstructionScreen'">
          <xsl:element name="Code">
            <xsl:value-of select="concat('InstructionBlocks.length - 1].AddScreen(new IATKeyedInstructionScreen(', position(), ', ', ContinueASCIIKeyCode, ', DisplayItem', ContinueInstructionsDisplayID, ', DisplayItem', InstructionsDisplayID, ', DisplayItem', LeftResponseDisplayID, ', DisplayItem', RightResponseDisplayID, '));')"/>
          </xsl:element>
        </xsl:when>
        <xsl:when test="@EventType eq 'MockItemInstructionScreen'">
          <xsl:element name="Code">
            <xsl:value-of select="concat('InstructionBlocks[InstructionBlocks.length - 1].AddScreen(new IATMockItemInstructionScreen(', position(), ', ', ContinueASCIIKeyCode, ', DisplayItem', ContinueInstructionsDisplayID, ', DisplayItem', LeftResponseDisplayID, ', DisplayItem', ./RightResponseDisplayID, ', DisplayItem', ./StimulusDisplayID, ', DisplayItem', ./InstructionsDisplayID, ', ', lower-case(ErrorMarkIsDisplayed), ', ', lower-case(OutlineLeftResponse), ', ', lower-case(OutlineRightResponse), '));')"/>
          </xsl:element>
        </xsl:when>
      </xsl:choose>
    </xsl:for-each>

  </xsl:template>



  <xsl:template name="GenerateEventInit">
    <xsl:param name="EventListNode"/>
    <xsl:param name="randomization"/>
    <xsl:element name="Code">ItemBlocks = new Array();</xsl:element>
    <xsl:element name="Code">InstructionBlocks = new Array();</xsl:element>
    <xsl:element name="Code">alternate = document.getElementById("Alternate").value;</xsl:element>
    <xsl:element name="Code">itemBlockCtr = 0;</xsl:element>
    <xsl:element name="Code">instructionBlockCtr = 0;</xsl:element>
    <xsl:element name="Code">numAlternatedItemBlocks = 0;</xsl:element>
    <xsl:element name="Code">numAlternatedInstructionBlocks = 0;</xsl:element>
    <xsl:element name="Code">GenerateEventList();</xsl:element>
    <xsl:variable name="blockStarters" select="('BeginIATBlock', 'BeginInstructionBlock')"/>
    <xsl:for-each select="$EventListNode/IATEvent[some $type in $blockStarters satisfies $type eq @EventType]">
      <xsl:choose>
        <xsl:when test="@EventType eq 'BeginIATBlock'">
          <xsl:element name="Code">if (alternate == "yes") {</xsl:element>
          <xsl:if test="./AlternatedWith ne '-1'">
            <xsl:choose>
              <xsl:when test="./AlternatedWith gt ./BlockNum">
                <xsl:element name="Code">
                  <xsl:value-of select="concat('itemBlock = ItemBlocks[', ./AlternatedWith, ' - (itemBlockCtr + 1)];')"/>
                </xsl:element>
                <xsl:element name="Code">
                  <xsl:value-of select="concat('ItemBlocks.splice(', ./AlternatedWith, ' - (itemBlockCtr + 1), 1);')"/>
                </xsl:element>
                <xsl:element name="Code">numAlternatedItemBlocks++;</xsl:element>
              </xsl:when>
              <xsl:when test="./AlternatedWith lt ./BlockNum">
                <xsl:element name="Code">
                  <xsl:value-of select="concat('itemBlock = ItemBlocks[', ./AlternatedWith, ' - (numAlternatedItemBlocks + 1)];')"/>
                </xsl:element>
                <xsl:element name="Code">
                  <xsl:value-of select="concat('ItemBlocks.splice(', ./AlternatedWith, ' - (numAlternatedItemBlocks + 1), 1);')"/>
                </xsl:element>
                <xsl:element name="Code">numAlternatedItemBlocks++;</xsl:element>
              </xsl:when>
            </xsl:choose>
          </xsl:if>
          <xsl:if test="./AlternatedWith eq '-1'">
            <xsl:element name="Code">itemBlock = ItemBlocks[numAlternatedItemBlocks];</xsl:element>
            <xsl:element name="Code">ItemBlocks.splice(numAlternatedItemBlocks, 1);</xsl:element>
          </xsl:if>
          <xsl:element name="Code">}</xsl:element>
          <xsl:element name="Code">else</xsl:element>
          <xsl:element name="Code">itemBlock = ItemBlocks.shift();</xsl:element>
          <xsl:element name="Code">
            <xsl:value-of select="concat('result = itemBlock.GenerateContents(&quot;', $randomization, '&quot;);')"/>
          </xsl:element>
          <xsl:element name="Code">while (result.length >  0)</xsl:element>
          <xsl:element name="Code">EventList.push(result.shift());</xsl:element>
          <xsl:element name="Code">itemBlockCtr++;</xsl:element>
        </xsl:when>
        <xsl:when test="@EventType eq 'BeginInstructionBlock'">
          <xsl:variable name="precedingNodes" select="preceding-sibling::node()"/>
          <xsl:variable name="blockNum"
                        select="count($precedingNodes[@EventType eq 'BeginInstructionBlock'])"
                        as="xs:integer"/>
          <xsl:variable name="Code">if (alternate == "yes") {</xsl:variable>
          <xsl:if test="./AlternatedWith ne '-1'">
            <xsl:choose>
              <xsl:when test="xs:integer(./AlternatedWith) gt $blockNum">
                <xsl:element name="Code">
                  <xsl:value-of select="concat('instructionBlock = InstructionBlocks[', ./AlternatedWith, ' - (instructionBlockCtr + 1)];')"/>
                </xsl:element>
                <xsl:element name="Code">
                  <xsl:value-of select="concat('InstructionBlocks.splice(', ./AlternatedWith, ' - (instructionBlockCtr + 1), 1);')"/>
                </xsl:element>
                <xsl:element name="Code">numAlternatedInstructionBlocks++;</xsl:element>
              </xsl:when>
              <xsl:when test="xs:integer(./AlternatedWith) lt $blockNum">
                <xsl:element name="Code">
                  <xsl:value-of select="concat('instructionBlock = InstructionBlocks[', ./AlternatedWith, ' - (numAlternatedInstructionBlocks + 2)];')"/>
                </xsl:element>
                <xsl:element name="Code">
                  <xsl:value-of select="concat('InstructionBlocks.splice(', ./AlternatedWith, ' - (numAlternatedInstructionBlocks + 2), 1);')"/>
                </xsl:element>
                <xsl:element name="Code">numAlternatedInstructionBlocks++;</xsl:element>
              </xsl:when>
            </xsl:choose>
          </xsl:if>
          <xsl:if test="./AlternatedWith eq '-1'">
            <xsl:element name="Code">instructionBlock = InstructionBlocks[numAlternatedInstructionBlocks];</xsl:element>
            <xsl:element name="Code">InstructionBlocks.splice(numAlternatedInstructionBlocks, 1);</xsl:element>
          </xsl:if>
          <xsl:element name="Code">} else</xsl:element>
          <xsl:element name="Code">instructionBlock = InstructionBlocks.shift();</xsl:element>
          <xsl:element name="Code">while (instructionBlock.screens.length > 0)</xsl:element>
          <xsl:element name="Code">EventList.push(instructionBlock.screens.shift());</xsl:element>
          <xsl:element name="Code">instructionBlockCtr++;</xsl:element>
        </xsl:when>
      </xsl:choose>
    </xsl:for-each>
    <xsl:element name="Code">EventList.push(new IATSubmitEvent(EventList.length));</xsl:element>
  </xsl:template>

  <xsl:template name="ProcessMaskSpecifiers">
    <xsl:element name="Code">var lesserArrayLength = (MaskItemTrueArray.length ? MaskItemFalseArray.length) ? MaskItemFalseArray.length : MaskItemTrueArray.length;</xsl:element>
    <xsl:element name="Code">for (ctr = 0; ctr &lt; lesserArrayLength; ctr++) {</xsl:element>
    <xsl:element name="Code">randomNum = Math.floor(Math.random() * MaskItemTrueArray.length);</xsl:element>
    <xsl:choose>
      <xsl:when test="(//Is7Block eq 'True') and (//RandomizationType eq 'SetNumberOfPresentations')">
        <xsl:element name="Code">if (MaskItemTrueArray[randomNum][3] == 1)</xsl:element>
        <xsl:element name="Code">Items1.push(new IATItem(FreeItemIDs.shift(), MaskItemTrueArray[randomNum][1], MaskItemTrueArray[randomNum][0], MaskItemTrueArray[randomNum][2]));</xsl:element>
        <xsl:element name="Code">else if (MaskItemTrueArray[randomNum][3] == 2)</xsl:element>
        <xsl:element name="Code">Items2.push(new IATItem(FreeItemIDs.shift(), MaskItemTrueArray[randomNum][1], MaskItemTrueArray[randomNum][0], MaskItemTrueArray[randomNum][2]));</xsl:element>
      </xsl:when>
      <xsl:otherwise>
        <xsl:element name="Code">Items.push(new IATItem(FreeItemIDs.shift(), MaskItemTrueArray[randomNum][1], MaskItemTrueArray[randomNum][0], MaskItemTrueArray[randomNum][2]));</xsl:element>
      </xsl:otherwise>
    </xsl:choose>
    <xsl:element name="Code">MaskItemTrueArray.splice(randomNum, 1);</xsl:element>
    <xsl:choose>
      <xsl:when test="(//Is7Block eq 'True') and (//RandomizationType eq 'SetNumberOfPresentations')">
        <xsl:element name="Code">randomNum = Math.floor(Math.random() * MaskItemFalseArray.length);</xsl:element>
        <xsl:element name="Code">if (MaskItemFalseArray[randomNum][3] == 1)</xsl:element>
        <xsl:element name="Code">Items1.push(new IATItem(FreeItemIDs.shift(), MaskItemFalseArray[ctr][1], MaskItemFalseArray[ctr][0], MaskItemFalseArray[ctr][2]));</xsl:element>
        <xsl:element name="Code">else if (MaskItemFalseArray[randomNum][3] == 2)</xsl:element>
        <xsl:element name="Code">Items2.push(new IATItem(FreeItemIDs.shift(), MaskItemFalseArray[ctr][1], MaskItemFalseArray[ctr][0], MaskItemFalseArray[ctr][2]));</xsl:element>
      </xsl:when>
      <xsl:otherwise>
        <xsl:element name="Code">Items.push(new IATItem(FreeItemIDs.shift(), MaskItemFalseArray[ctr][1], MaskItemFalseArray[ctr][0], MaskItemFalseArray[ctr][2]));</xsl:element>
      </xsl:otherwise>
    </xsl:choose>
    <xsl:element name="Code">MaskItemFalseArray.splice(randomNum, 1); }</xsl:element>
  </xsl:template>


  <xsl:template name="ProcessNoSpecItem" match="IATEvent[SpecifierID eq '-1']">
    <xsl:variable name="item" select="."/>
    <xsl:variable name="specifier" select="//DynamicSpecifier[ID eq $item/SpecifierID]"/>
    <xsl:element name="Code">
      <xsl:value-of select="concat('KeyedDir = &quot;', ./KeyedDir, '&quot;;')"/>
    </xsl:element>
    <xsl:variable name="params"
                  select="concat(ItemNum, ', ', 'DisplayItem', ./StimulusDisplayID, ', ', ./ItemNum, ',  KeyedDir')"/>
    <xsl:choose>
      <xsl:when test="(//Is7Block eq 'True') and (//RandomizationType eq 'SetNumberOfPresentations')">
        <xsl:if test="OriginatingBlock eq '1'">
          <xsl:element name="Code">
            <xsl:value-of select="concat('Items1.push(new IATItem(', $params, '));')"/>
          </xsl:element>
        </xsl:if>
        <xsl:if test="OriginatingBlock eq '2'">
          <xsl:element name="Code">
            <xsl:value-of select="concat('Items2.push(new IATItem(', $params, '));')"/>
          </xsl:element>
        </xsl:if>
      </xsl:when>
      <xsl:otherwise>
        <xsl:element name="Code">
          <xsl:value-of select="concat('Items.push(new IATItem(', $params, '));')"/>
        </xsl:element>
      </xsl:otherwise>
    </xsl:choose>
  </xsl:template>

  <xsl:template name="ProcessTrueFalseSpecItem" match="IATEvent[some $s in //DynamicSpecifier satisfies ($s/ID eq SpecifierID and $s/@SpecifierType eq 'TrueFalse')]">
    <xsl:if test="./KeyedDir eq 'DynamicRight'">
      <xsl:element name="Code">
        <xsl:value-of select="'DefaultKey = &quot;Right&quot;;'"/>
      </xsl:element>
    </xsl:if>
    <xsl:if test="./KeyedDir eq 'DynamicLeft'">
      <xsl:element name="Code">
        <xsl:value-of select="'DefaultKey = &quot;Left&quot;;'"/>
      </xsl:element>
    </xsl:if>
    <xsl:variable name="item" select="."/>
    <xsl:variable name="specifier" select="//DynamicSpecifiers[ID eq $item/SpecifierID]"/>
    <xsl:element name="Code">
      <xsl:value-of select="concat('KeyedDirInput = document.getElementById(DynamicKey', $specifier/ID, ');')"/>
    </xsl:element>
    <xsl:element name="Code">if (KeyedDirInput.value == "True")</xsl:element>
    <xsl:element name="Code">KeyedDir = DefaultKey;</xsl:element>
    <xsl:element name="Code">else if (DefaultKey == "Right")</xsl:element>
    <xsl:element name="Code">KeyedDir = "Left";</xsl:element>
    <xsl:element name="Code">else</xsl:element>
    <xsl:element name="Code">KeyedDir = "Right";</xsl:element>
    <xsl:variable name="params"
                  select="concat(ItemNum, ', ', 'DisplayItem', ./StimulusDisplayID, ', ', $specifier/ItemNum, ', KeyedDir')"/>
    <xsl:choose>
      <xsl:when test="(//Is7Block eq 'True') and (//RandomizationType eq 'SetNumberOfPresentations')">
        <xsl:if test="OriginatingBlock eq '1'">
          <xsl:element name="Code">
            <xsl:value-of select="concat('Items1.push(new IATItem(', $params, '));')"/>
          </xsl:element>
        </xsl:if>
        <xsl:if test="OriginatingBlock eq '2'">
          <xsl:element name="Code">
            <xsl:value-of select="concat('Items2.push(new IATItem(', $params, '));')"/>
          </xsl:element>
        </xsl:if>
      </xsl:when>
      <xsl:otherwise>
        <xsl:element name="Code">
          <xsl:value-of select="concat('Items.push(new IATItem(', $params, '));')"/>
        </xsl:element>
      </xsl:otherwise>
    </xsl:choose>
  </xsl:template>

  <xsl:template name="ProcessRangeSpecItem" match="IATEvent[some $s in //DynamicSpecifier satisfies ($s/ID eq SpecifierID and $s/@SpecifierType eq 'Range')]" >
    <xsl:if test="./KeyedDir eq 'DynamicRight'">
      <xsl:element name="Code">
        <xsl:value-of select="'DefaultKey = &quot;Right&quot;;'"/>
      </xsl:element>
    </xsl:if>
    <xsl:if test="./KeyedDir eq 'DynamicLeft'">
      <xsl:element name="Code">
        <xsl:value-of select="'DefaultKey = &quot;Left&quot;;'"/>
      </xsl:element>
    </xsl:if>
    <xsl:variable name="item" select="."/>
    <xsl:variable name="specifier" select="//DynamicSpecifiers[ID eq $item/SpecifierID]"/>
    <xsl:element name="Code">
      <xsl:value-of select="concat('KeyedDirInput = document.getElementById(DynamicKey', $specifier/ID, ');')"/>
    </xsl:element>
    <xsl:element name="Code">if (KeyedDirInput.value != "Exclude") {</xsl:element>
    <xsl:element name="Code">if (KeyedDirInput.value == "True")</xsl:element>
    <xsl:element name="Code">KeyedDir = DefaultKey;</xsl:element>
    <xsl:element name="Code">else if (DefaultKey == "Right")</xsl:element>
    <xsl:element name="Code">KeyedDir = "Left";</xsl:element>
    <xsl:element name="Code">else</xsl:element>
    <xsl:element name="Code">KeyedDir = "Right";</xsl:element>
    <xsl:variable name="params"
                  select="concat(ItemNum, ', ', 'DisplayItem', ./StimulusDisplayID, ', ', $specifier/ItemNum, ', ', KeyedDir)"/>
    <xsl:choose>
      <xsl:when test="(//Is7Block eq 'True') and (//RandomizationType eq 'SetNumberOfPresentations')">
        <xsl:if test="OriginatingBlock eq '1'">
          <xsl:element name="Code">
            <xsl:value-of select="concat('Items1.push(new IATItem(', $params, '));')"/>
          </xsl:element>
        </xsl:if>
        <xsl:if test="OriginatingBlock eq '2'">
          <xsl:element name="Code">
            <xsl:value-of select="concat('Items2.push(new IATItem(', $params, '));')"/>
          </xsl:element>
        </xsl:if>
      </xsl:when>
      <xsl:otherwise>
        <xsl:element name="Code">
          <xsl:value-of select="concat('Items.push(new IATItem(', $params, '));')"/>
        </xsl:element>
      </xsl:otherwise>
    </xsl:choose>
    <xsl:element name="Code">}</xsl:element>
  </xsl:template>

  <xsl:template name="ProcessMaskSpecItem" match="IATEvent[some $s in //DynamicSpecifier satisfies ($s/ID eq SpecifierID and $s/@SpecifierType eq 'Mask')] ">
    <xsl:if test="./KeyedDir eq 'DynamicRight'">
      <xsl:element name="Code">
        <xsl:value-of select="'DefaultKey = &quot;Right&quot;;'"/>
      </xsl:element>
    </xsl:if>
    <xsl:if test="./KeyedDir eq 'DynamicLeft'">
      <xsl:element name="Code">
        <xsl:value-of select="'DefaultKey = &quot;Left&quot;;'"/>
      </xsl:element>
    </xsl:if>
    <xsl:element name="Code">
      <xsl:value-of select="'FreeItemIDs.push(', ItemNum, ');'"/>
    </xsl:element>
    <xsl:variable name="items" select="."/>
    <xsl:for-each-group select="//DynamicSpecifier[(some $i in $items satisfies $i/SpecifierID eq ./ID) and (@SpecifierType eq 'Mask')]"
                        group-by="SurveyName">
      <xsl:for-each-group select="//DynamicSpecifier[(some $i in $items satisfies $i/SpecifierID eq ./ID) and (@SpecifierType eq 'Mask')]"
                          group-by="ItemNum">
        <xsl:element name="Code">MaskItemTrueArray = new Array();</xsl:element>
        <xsl:element name="Code">MaskItemFalseArray = new Array();</xsl:element>
        <xsl:for-each select="current-group()">
          <xsl:variable name="specificSpecifier" select="."/>
          <xsl:variable name="itemList"
                        select="$items[($specificSpecifier/ID eq SpecifierID) and (@EventType eq 'IATItem')]"/>
          <xsl:if test="count($itemList) eq 1">
            <xsl:call-template name="MaskSpecifierArrayAppend">
              <xsl:with-param name="item" select="$itemList"/>
            </xsl:call-template>
          </xsl:if>
          <xsl:if test="count($itemList) gt 1">
            <xsl:call-template name="MaskSpecifierArrayAppendRange">
              <xsl:with-param name="itemList" select="$itemList"/>
            </xsl:call-template>
          </xsl:if>
        </xsl:for-each>
      </xsl:for-each-group>
    </xsl:for-each-group>
  </xsl:template>

  <xsl:template name="ProcessSelectionSpecItem" match="IATEvent[some $s in //DynamicSpecifier satisfies ($s/ID eq SpecifierID and $s/@SpecifierType eq 'Selection')] ">
    <xsl:if test="./KeyedDir eq 'DynamicRight'">
      <xsl:element name="Code">
        <xsl:value-of select="'DefaultKey = &quot;Right&quot;;'"/>
      </xsl:element>
    </xsl:if>
    <xsl:if test="./KeyedDir eq 'DynamicLeft'">
      <xsl:element name="Code">
        <xsl:value-of select="'DefaultKey = &quot;Left&quot;;'"/>
      </xsl:element>
    </xsl:if>
    <xsl:element name="Code">
      <xsl:value-of select="'FreeItemIDs.push(', ItemNum, ');'"/>
    </xsl:element>
    <xsl:variable name="item" select="."/>
    <xsl:variable name="specifier" select="//DynamicSpecifiers[ID eq $item/SpecifierID]"/>
    <xsl:value-of select="concat('SelectedItem = document.getElementById(DynamicKey', $specifier/ID, ').value;')"/>
    <xsl:element name="Code">RandomItem = SelectedItem;</xsl:element>
    <xsl:element name="Code">SelectionStimulusArray = new Array();</xsl:element>
    <xsl:variable name="items"
                  select="//IATEvent[(@Type eq 'IATItem') and ($item/SpecifierID eq SpecifierID)]"/>
    <xsl:for-each select="$items">
      <xsl:variable name="params"
                    select="concat(''', ./SpecifierArg, '' , DisplayItem', ./StimulusDisplayID, ', '', ./KeyedDir, '', ', ./ItemNum, ', ', ./OriginatingBlock)"/>
      <xsl:value-of select="concat('SelectionStimulusArray.push(new Array(', $params, '));')"/>
    </xsl:for-each>
    <xsl:element name="Code">for (ctr = 0; ctr &lt; SelectionStimulusArray.length; ctr++) {</xsl:element>
    <xsl:element name="Code">if (SelectedItem == SelectionStimulusArray[ctr][0]) {</xsl:element>
    <xsl:element name="Code">SelectionKeyedDir = SelectionStimulusArray[ctr][2];</xsl:element>
    <xsl:element name="Code">SelectionStimulus = SelectionStimulusArray[ctr][1];</xsl:element>
    <xsl:element name="Code">SelectionItemNum = SelectionStimulusArray[ctr][3];</xsl:element>
    <xsl:element name="Code">SelectionOriginatingBlock = SelectionStimulusArray[ctr][4];</xsl:element>
    <xsl:element name="Code">if (SelectionKeyedDir == "DynamicLeft")</xsl:element>
    <xsl:element name="Code">SelectionKeyedDir = "Left";</xsl:element>
    <xsl:element name="Code">else</xsl:element>
    <xsl:element name="Code">SelectionKeyedDir = "Right";</xsl:element>
    <xsl:element name="Code">}</xsl:element>
    <xsl:element name="Code">}</xsl:element>
    <xsl:choose>
      <xsl:when test="(//Is7Block eq 'True') and (//RandomizationType eq 'SetNumberOfPresentations')">
        <xsl:element name="Code">if (SelectionOriginatingBlock  == 1)</xsl:element>
        <xsl:element name="Code">Items1.push(new IATItem(FreeItemIDs.shift(), SelectionStimulus, SelectionItemNum, SelectionKeyedDir));</xsl:element>
        <xsl:element name="Code">else if (SelectionOriginatingBlock == 2)</xsl:element>
        <xsl:element name="Code">Items2.push(new IATItem(FreeItemIDs.shift(), SelectionStimulus, SelectionItemNum, SelectionKeyedDir));</xsl:element>
      </xsl:when>
      <xsl:otherwise>
        <xsl:element name="Code">Items.push(new IATItem(FreeItemIDs.shift(), SelectionStimulus, SelectionItemNum, SelectionKeyedDir));</xsl:element>
      </xsl:otherwise>
    </xsl:choose>
    <xsl:element name="Code">RandomItem = SelectedItem;</xsl:element>
    <xsl:element name="Code">while (RandomItem == SelectedItem) {</xsl:element>
    <xsl:value-of select="concat('randomNum  = Math.floor(Math.random() * ', count(./KeySpecifiers/KeySpecifier), ');')"/>
    <xsl:for-each select="$specifier/KeySpecifiers/KeySpecifier">
      <xsl:element name="Code">
        <xsl:value-of select="concat('if (randomNum == ', count(preceding-sibling), ')')"/>
      </xsl:element>
      <xsl:element name="Code">
        <xsl:value-of select="concat('RandomItem = &quot;', ., '&quot;')"/>
      </xsl:element>
    </xsl:for-each>
    <xsl:element name="Code">}</xsl:element>
    <xsl:element name="Code">for (ctr = 0; ctr &lt; SelectionStimulusArray.length; ctr++)</xsl:element>
    <xsl:element name="Code">if (RandomItem == SelectionStimulusArray[ctr][0])</xsl:element>
    <xsl:element name="Code">{</xsl:element>
    <xsl:element name="Code">SelectionKeyedDir = SelectionStimulusArray[ctr][2];</xsl:element>
    <xsl:element name="Code">SelectionStimulus = SelectionStimulusArray[ctr][1];</xsl:element>
    <xsl:element name="Code">SelectionItemNum = SelectionStimulusArray[ctr][3];</xsl:element>
    <xsl:element name="Code">SelectionOriginatingBlock = SelectionStimulusArray[ctr][4];</xsl:element>
    <xsl:element name="Code">if (SelectionKeyedDir == "DynamicLeft")</xsl:element>
    <xsl:element name="Code">SelectionKeyedDir = "Right";</xsl:element>
    <xsl:element name="Code">else</xsl:element>
    <xsl:element name="Code">SelectionKeyedDir = "Left";</xsl:element>
    <xsl:element name="Code">}</xsl:element>
    <xsl:choose>
      <xsl:when test="(//Is7Block eq 'True') and (//RandomizationType eq 'SetNumberOfPresentations')">
        <xsl:element name="Code">if (SelectionOriginatingBlock == 1)</xsl:element>
        <xsl:element name="Code">Items1.push(new IATItem(FreeItemIDs.shift(), SelectionStimulus, SelectionItemNum, SelectionKeyedDir));</xsl:element>
        <xsl:element name="Code">else if (SelectionOriginatingBlock == 2)</xsl:element>
        <xsl:element name="Code">Items2.push(new IATItem(FreeItemIDs.shift(), SelectionStimulus, SelectionItemNum, SelectionKeyedDir));</xsl:element>
      </xsl:when>
      <xsl:otherwise>
        <xsl:element name="Code">Items.push(new IATItem(FreeItemIDs.shift(), SelectionStimulus, SelectionItemNum, SelectionKeyedDir));</xsl:element>
      </xsl:otherwise>
    </xsl:choose>
  </xsl:template>


  <xsl:template name="GenerateProcessItemFunctions">
    <xsl:for-each select="//IATEvent[@EventType eq 'BeginIATBlock']">
      <xsl:variable name="blockLen" select="NumItems" />
      <xsl:variable name="items" select="following::IATEvent[position() le xs:integer($blockLen)]" />
      <xsl:element name="Function">
        <xsl:attribute name="FunctionName" select="concat('ProcessItems', position())" />
        <xsl:element name="Params"/>
        <xsl:variable name="functionBodyElems">
          <xsl:apply-templates select="$items" />
        </xsl:variable>
        <xsl:element name="FunctionBody">
          <xsl:for-each select="$functionBodyElems/Code">
            <xsl:element name="Code">
              <xsl:attribute name="LineNum" select="position()" />
              <xsl:value-of select="." />
            </xsl:element>
          </xsl:for-each>
        </xsl:element>
      </xsl:element>
    </xsl:for-each>

  </xsl:template>

  <xsl:template name="GenerateBlockLoad">
    <xsl:param name="items"/>
    <xsl:param name="startPosition" as="xs:integer"/>

    <xsl:choose>
      <xsl:when test="(//Is7Block eq 'True') and (//RandomizationType eq 'SetNumberOfPresentations')">
        <xsl:element name="Code">Items1 = new Array();</xsl:element>
        <xsl:element name="Code">Items2 = new Array();</xsl:element>
      </xsl:when>
      <xsl:otherwise>
        <xsl:element name="Code">Items = new Array();</xsl:element>
      </xsl:otherwise>
    </xsl:choose>
    <xsl:element name="Code">FreeItemIDs = new Array();</xsl:element>
    <xsl:element name="Code">
      <xsl:value-of select="concat('ProcessItems', count(//IATEvent[(@EventType eq 'BeginIATBlock') and (position() lt $startPosition)]), '();')" />
    </xsl:element>
    <!---    <xsl:apply-templates select="$items" />  -->
    <xsl:choose>
      <xsl:when test="(//Is7Block eq 'True') and (//RandomizationType eq 'SetNumberOfPresentations')">
        <xsl:element name="Code">if (Items2.length == 0) {</xsl:element>
        <xsl:element name="Code">for (ctr = 0; ctr &lt; Items1.length; ctr++)</xsl:element>
        <xsl:element name="Code">ItemBlocks[ItemBlocks.length - 1].Items.push(Items1[ctr]);</xsl:element>
        <xsl:element name="Code">} else if (Items1.length == 0)  {</xsl:element>
        <xsl:element name="Code">for (ctr = 0; ctr &lt; Items2.length; ctr++)</xsl:element>
        <xsl:element name="Code">ItemBlocks[ItemBlocks.length - 1].Items.push(Items2[ctr]);</xsl:element>
        <xsl:element name="Code">} else if (Items1.length == 0) {</xsl:element>
        <xsl:element name="Code">for (ctr = 0; ctr &lt; Items2.length; ctr++) {</xsl:element>
        <xsl:element name="Code">ItemBlocks[ItemBlocks.length - 1].Items.push(Items2[ctr]);</xsl:element>
        <xsl:element name="Code">}</xsl:element>
        <xsl:element name="Code">}</xsl:element>
        <xsl:element name="Code">ctr = 0;</xsl:element>
        <xsl:element name="Code">while ((ctr &lt; Items1.length) &amp;&amp; (ctr &lt; Items2.length)) {</xsl:element>
        <xsl:element name="Code">if (Items1.length &gt; Items2.length) {</xsl:element>
        <xsl:element name="Code">randomNum = Math.floor(Math.random() * Items1.length);</xsl:element>
        <xsl:element name="Code">ItemBlocks[ItemBlocks.length - 1].Items.push(Items1[randomNum]);</xsl:element>
        <xsl:element name="Code">Items1.splice(randomNum, 1);</xsl:element>
        <xsl:element name="Code">ItemBlocks[ItemBlocks.length - 1].Items.push(Items2[0]);</xsl:element>
        <xsl:element name="Code">Items2.splice(0, 1);</xsl:element>
        <xsl:element name="Code">} else {</xsl:element>
        <xsl:element name="Code">randomNum = Math.floor(Math.random() * Items2.length);</xsl:element>
        <xsl:element name="Code">ItemBlocks[ItemBlocks.length - 1].Items.push(Items1[0]);</xsl:element>
        <xsl:element name="Code">Items1.splice(0, 1);</xsl:element>
        <xsl:element name="Code">ItemBlocks[ItemBlocks.length - 1].Items.push(Items2[randomNum]);</xsl:element>
        <xsl:element name="Code">Items2.splice(randomNum, 1);</xsl:element>
        <xsl:element name="Code">}</xsl:element>
        <xsl:element name="Code">}</xsl:element>
      </xsl:when>
      <xsl:otherwise>
        <xsl:element name="Code">for (ctr = 0; ctr &lt; Items.length; ctr++)</xsl:element>
        <xsl:element name="Code">ItemBlocks[ItemBlocks.length - 1].Items.push(Items[ctr]);</xsl:element>
      </xsl:otherwise>
    </xsl:choose>
  </xsl:template>

  <xsl:template name="OutputConstructorDefinition">
    <xsl:param name="class"/>
    <xsl:variable name="params" select="$class/Params"/>
    <xsl:if test="count($params) gt 0">
      <xsl:variable name="paramList" select="string-join($params/Param, ', ')"/>
      <xsl:element name="CodeLine">
        <xsl:attribute name="Type" select="'ClassStart'"/>
        <xsl:attribute name="Name" select="$class/@ClassName"/>
        <xsl:value-of select="concat('function ', $class/@ClassName, '(', $paramList, ') {')"/>
      </xsl:element>
    </xsl:if>
    <xsl:if test="count($params) eq 0">
      <xsl:element name="CodeLine">
        <xsl:attribute name="Type" select="'ClassStart'"/>
        <xsl:attribute name="Name" select="$class/@ClassName"/>
        <xsl:value-of select="concat('function ', $class/@ClassName, '() {')"/>
      </xsl:element>
    </xsl:if>
  </xsl:template>

  <xsl:template name="OutputConstructorBody">
    <xsl:param name="class"/>
    <xsl:for-each select="$class/Constructor/ConstructorBody/Code">
      <xsl:element name="CodeLine">
        <xsl:attribute name="Type" select="'Code'"/>
        <xsl:value-of select="."/>
      </xsl:element>
    </xsl:for-each>
    <xsl:element name="CodeLine">
      <xsl:attribute name="Type" select="'Code'"/>
      <xsl:value-of select="'}'"/>
    </xsl:element>
  </xsl:template>

  <xsl:template name="OutputMemberFunctionDefinition">
    <xsl:param name="function"/>
    <xsl:param name="className"/>
    <xsl:variable name="paramList" select="string-join($function/Params/Param, ', ')"/>
    <xsl:element name="CodeLine">
      <xsl:attribute name="Type" select="'FunctionStart'"/>
      <xsl:attribute name="Name" select="concat($className, '.', $function/@FunctionName)"/>
      <xsl:value-of select="concat($className, '.prototype.', $function/@FunctionName, ' = function(', $paramList, ') {')"/>
    </xsl:element>
  </xsl:template>

  <xsl:template name="OutputFunctionDefinition">
    <xsl:param name="function"/>
    <xsl:variable name="params" select="$function/Params"/>
    <xsl:variable name="paramList" select="string-join($function/Params/Param, ', ')" />
    <xsl:element name="CodeLine">
      <xsl:attribute name="Type" select="'FunctionStart'"/>
      <xsl:attribute name="Name" select="$function/@FunctionName"/>
      <xsl:value-of select="concat('function ', $function/@FunctionName, '(', $paramList, ') {')"/>
    </xsl:element>
  </xsl:template>

  <xsl:template name="OutputFunctionBody">
    <xsl:param name="function"/>
    <xsl:for-each select="$function/FunctionBody/Code">
      <xsl:element name="CodeLine">
        <xsl:attribute name="Type" select="'FunctionCode'"/>
        <xsl:attribute name="Name" select="$function/@FunctionName"/>
        <xsl:value-of select="."/>
      </xsl:element>
    </xsl:for-each>
  </xsl:template>

  <xsl:template name="OutputPrototypeChain">
    <xsl:param name="class"/>
    <xsl:variable name="prototype" select="$class/PrototypeChain"/>
    <xsl:element name="CodeLine">
      <xsl:attribute name="Type" select="'ConstructorDefinition'"/>
      <xsl:attribute name="Name" select="$class/@ClassName"/>
      <xsl:value-of select="concat($class/@ClassName, '.prototype.constructor = ', $class/@ClassName, ';')"/>
    </xsl:element>
    <xsl:for-each select="$class/PrototypeChain/Function">
      <xsl:call-template name="OutputMemberFunctionDefinition">
        <xsl:with-param name="function" select="."/>
        <xsl:with-param name="className" select="$class/@ClassName"/>
      </xsl:call-template>
      <xsl:call-template name="OutputFunctionBody">
        <xsl:with-param name="function" select="."/>
      </xsl:call-template>
      <xsl:element name="CodeLine">
        <xsl:attribute name="Type" select="'FunctionEnd'"/>
        <xsl:attribute name="Name" select="concat($class/@ClassName, '.', @FunctionName)"/>
        <xsl:value-of select="'};'"/>
      </xsl:element>
    </xsl:for-each>
  </xsl:template>

  <xsl:template name="OutputClass">
    <xsl:param name="class"/>
    <xsl:call-template name="OutputConstructorDefinition">
      <xsl:with-param name="class" select="$class"/>
    </xsl:call-template>
    <xsl:call-template name="OutputConstructorBody">
      <xsl:with-param name="class" select="$class"/>
    </xsl:call-template>
    <xsl:call-template name="OutputPrototypeChain">
      <xsl:with-param name="class" select="$class"/>
    </xsl:call-template>
    <xsl:element name="CodeLine">
      <xsl:attribute name="Type" select="'ClassEnd'"/>
      <xsl:attribute name="Name" select="$class/@ClassName"/>
    </xsl:element>
  </xsl:template>

  <xsl:template name="OutputFunction">
    <xsl:param name="function"/>
    <xsl:call-template name="OutputFunctionDefinition">
      <xsl:with-param name="function" select="."/>
    </xsl:call-template>
    <xsl:call-template name="OutputFunctionBody">
      <xsl:with-param name="function" select="."/>
    </xsl:call-template>
    <xsl:element name="CodeLine">
      <xsl:attribute name="Type" select="'FunctionEnd'"/>
      <xsl:attribute name="Name" select="$function/@FunctionName"/>
      <xsl:value-of select="'}'"/>
    </xsl:element>
  </xsl:template>

</xsl:stylesheet>