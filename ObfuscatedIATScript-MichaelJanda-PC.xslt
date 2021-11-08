<?xml version="1.0" encoding="UTF-8"?>
<xsl:stylesheet xmlns:xs="http://www.w3.org/2001/XMLSchema"
                xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
                version="2.0"
                exclude-result-prefixes="xs">

  <xsl:output method="xml" encoding="utf-8" indent="yes"/>

  <xsl:variable name="variableDeclarations">
    <Declarations>
      <Declaration>var inPracticeBlock = false;</Declaration>
      <Declaration>var itemBlock;</Declaration>
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
      <Declaration>var KeyedDirInput;</Declaration>
      <Declaration>var Display;</Declaration>
      <Declaration>var DefaultKey;</Declaration>
      <Declaration>var FreeItemIDs;</Declaration>
      <Declaration>var Items;</Declaration>
      <Declaration>var Items1;</Declaration>
      <Declaration>var Items2;</Declaration>
      <Declaration>var ctr;</Declaration>
      <xsl:for-each select="//DisplayItemList/IATDisplayItem">
        <Declaration>
          <xsl:value-of select="concat('var DI', ./ID, ';')"/>
        </Declaration>
      </xsl:for-each>
      <Declaration>var instructionBlock;</Declaration>
      <Declaration>var InstructionBlocks;</Declaration>
      <Declaration>var alternate;</Declaration>
      <Declaration>var itemBlockCtr, itemCtr = 0;</Declaration>
      <Declaration>var instructionsBlockCtr;</Declaration>
      <Declaration>var numAlternatedItemBlocks;</Declaration>
      <Declaration>var numAlternatedInstructionBlocks;</Declaration>
      <Declaration>var processIATItemFunctions = new Array();</Declaration>
    </Declarations>
  </xsl:variable>


  <xsl:variable name="GlobalAbbreviations">
    <xsl:variable name="Globals" select="string-join(for $elem in $variableDeclarations/Declarations/Declaration return replace($elem, '^var\s+(.+);$', '$1'), ', ')" />
    <xsl:analyze-string select="$Globals" regex="([A-Za-z_][A-Za-z0-9_]*)(\s*(=((\s+|[^;=/,&#34;\(]+?|&#34;[^&#34;\n\r]*?&#34;|\(([^;=,&#34;]*?,?(&#34;[^\n\r&#34;]*?&#34;)?)+\)|/[^/\n]+?/)*)+?)?)">
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
            <xsl:value-of select="normalize-space(regex-group(4))" />
          </xsl:element>
        </xsl:element>
      </xsl:matching-substring>
    </xsl:analyze-string>
  </xsl:variable>


  <xsl:variable name="functionPrefix">
    <xsl:value-of select="'iF'"/>
  </xsl:variable>

  <xsl:variable name="globalVariablePrefix">
    <xsl:value-of select="'iG'"/>
  </xsl:variable>

  <xsl:variable name="classPrefix">
    <xsl:value-of select="'iC'"/>
  </xsl:variable>

  <xsl:variable name="classFunctionPrefix">
    <xsl:value-of select="'iCF'"/>
  </xsl:variable>

  <xsl:variable name="Classes">
    <xsl:element name="Class">
      <xsl:attribute name="ClassName" select="'IATDI'"/>
      <xsl:element name="Super">
        <xsl:attribute name="Has" select="'no'" />
      </xsl:element>
      <xsl:element name="Constructor">
        <xsl:element name="Params">
          <xsl:element name="Param">id</xsl:element>
          <xsl:element name="Param">img</xsl:element>
          <xsl:element name="Param">x</xsl:element>
          <xsl:element name="Param">y</xsl:element>
          <xsl:element name="Param">width</xsl:element>
          <xsl:element name="Param">height</xsl:element>
        </xsl:element>
        <xsl:variable name="constructorBodyElems">
          <xsl:element name="Code">this.id = id;</xsl:element>
          <xsl:element name="Code">this.imgTag = img;</xsl:element>
          <xsl:element name="Code">this.x = x;</xsl:element>
          <xsl:element name="Code">this.y = y;</xsl:element>
          <xsl:element name="Code">this.width = width;</xsl:element>
          <xsl:element name="Code">this.height = height;</xsl:element>
          <xsl:element name="Code">this.divTag = document.createElement("div");</xsl:element>
          <xsl:element name="Code">this.divTag.id = "IATDI" + id.toString();</xsl:element>
          <xsl:element name="Code">return this;</xsl:element>
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
        <!--
        <xsl:element name="Function">
          <xsl:attribute name="FunctionName" select="'getImgTagID'" />
          <xsl:element name="Params" />
          <xsl:element name="FunctionBody">
            <xsl:element name="Code">return this.imgTag.id</xsl:element>
          </xsl:element>
        </xsl:element>
-->
        <xsl:element name="Function">
          <xsl:attribute name="FunctionName" select="'getId'" />
          <xsl:element name="Params" />
          <xsl:element name="FunctionBody">
            <xsl:element name="Code">return this.id</xsl:element>
          </xsl:element>
        </xsl:element>

        <!--
        <xsl:element name="Function">
          <xsl:attribute name="FunctionName" select="'setImgTagID'" />
          <xsl:element name="Params">
            <xsl:element name="Param">ImgTagID</xsl:element>
          </xsl:element>
          <xsl:element name="FunctionBody">
            <xsl:element name="Code">this.imgTag.id = ImgTagID;</xsl:element>
          </xsl:element>
        </xsl:element>
-->
        <xsl:element name="Function">
          <xsl:attribute name="FunctionName" select="'Outline'"/>
          <xsl:element name="Params"/>
          <xsl:element name="FunctionBody">
            <xsl:element name="Code">this.divTag.className = "outlinedDI";</xsl:element>
          </xsl:element>
        </xsl:element>

        <xsl:element name="Function">
          <xsl:attribute name="FunctionName" select="'Display'"/>
          <xsl:element name="Params">
            <xsl:element name="Param">parentNode</xsl:element>
          </xsl:element>
          <xsl:element name="FunctionBody">
            <xsl:element name="Code">this.divTag.appendChild(this.imgTag);</xsl:element>
            <xsl:element name="Code">parentNode.appendChild(this.divTag);</xsl:element>
          </xsl:element>
        </xsl:element>

        <xsl:element name="Function">
          <xsl:attribute name="FunctionName" select="'Hide'"/>
          <xsl:element name="Params"/>
          <xsl:variable name="functionBodyElems">
            <xsl:element name="Code">if (this.divTag.parentNode) {</xsl:element>
            <xsl:element name="Code">this.divTag.parentNode.removeChild(this.divTag);</xsl:element>
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
      <xsl:element name="Super">
        <xsl:attribute name="Has" select="'no'" />
      </xsl:element>
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
            <xsl:value-of select="concat('this.leftResponseKeyCodeUpper = ', //LeftResponseASCIIKeyCodeUpper, ';')"/>
          </xsl:element>
          <xsl:element name="Code">
            <xsl:value-of select="concat('this.rightResponseKeyCodeUpper = ', //RightResponseASCIIKeyCodeUpper, ';')"/>
          </xsl:element>
          <xsl:element name="Code">
            <xsl:value-of select="concat('this.leftResponseKeyCodeLower = ', //LeftResponseASCIIKeyCodeLower, ';')"/>
          </xsl:element>
          <xsl:element name="Code">
            <xsl:value-of select="concat('this.rightResponseKeyCodeLower = ', //RightResponseASCIIKeyCodeLower, ';')"/>
          </xsl:element>
          <xsl:element name="Code">this.divTag  = document.getElementById("IATDisplayDiv");</xsl:element>
          <xsl:element name="Code">while (this.divTag.hasChildNodes())</xsl:element>
          <xsl:element name="Code">this.divTag.removeChild(this.divTag.firstChild);</xsl:element>
          <xsl:element name="Code">this.displayItems = new Array();</xsl:element>
          <xsl:element name="Code">return this;</xsl:element>
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
            <xsl:element name="Code">this.displayItems.push(di);</xsl:element>
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
          <xsl:attribute name="FunctionName" select="'getDivTag'"/>
          <xsl:element name="Params" />
          <xsl:element name="FunctionBody">
            <xsl:element name="Code">return this.divTag;</xsl:element>
          </xsl:element>
        </xsl:element>

        <xsl:element name="Function">
          <xsl:attribute name="FunctionName" select="'getRightResponseKeyCodeLower'"/>
          <xsl:element name="Params" />
          <xsl:element name="FunctionBody">
            <xsl:element name="Code">return this.rightResponseKeyCodeLower;</xsl:element>
          </xsl:element>
        </xsl:element>

        <xsl:element name="Function">
          <xsl:attribute name="FunctionName" select="'getRightResponseKeyCodeUpper'"/>
          <xsl:element name="Params" />
          <xsl:element name="FunctionBody">
            <xsl:element name="Code">return this.rightResponseKeyCodeUpper;</xsl:element>
          </xsl:element>
        </xsl:element>

        <xsl:element name="Function">
          <xsl:attribute name="FunctionName" select="'getLeftResponseKeyCodeLower'"/>
          <xsl:element name="Params" />
          <xsl:element name="FunctionBody">
            <xsl:element name="Code">return this.leftResponseKeyCodeLower;</xsl:element>
          </xsl:element>
        </xsl:element>

        <xsl:element name="Function">
          <xsl:attribute name="FunctionName" select="'getLeftResponseKeyCodeUpper'"/>
          <xsl:element name="Params" />
          <xsl:element name="FunctionBody">
            <xsl:element name="Code">return this.leftResponseKeyCodeUpper;</xsl:element>
          </xsl:element>
        </xsl:element>


        <xsl:element name="Function">
          <xsl:attribute name="FunctionName" select="'RemoveDisplayItem'"/>
          <xsl:element name="Params">
            <xsl:element name="Param">di</xsl:element>
          </xsl:element>
          <xsl:variable name="functionBodyElems">
            <xsl:element name="Code">for (var ctr = 0; ctr &lt; this.displayItems.length; ctr++) {</xsl:element>
            <xsl:element name="Code">if (this.displayItems[ctr].getId() == di.getId()) {</xsl:element>
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
          <xsl:element name="FunctionBody">
            <xsl:element name="Code">this.startTime = (new Date()).getTime();</xsl:element>
          </xsl:element>
        </xsl:element>

        <xsl:element name="Function">
          <xsl:attribute name="FunctionName" select="'StopTimer'"/>
          <xsl:element name="Params" />
          <xsl:element name="FunctionBody">
            <xsl:element name="Code">return (new Date()).getTime() - this.startTime;</xsl:element>
          </xsl:element>
        </xsl:element>

      </xsl:element>
    </xsl:element>

    <xsl:element name="Class">
      <xsl:attribute name="ClassName" select="'IATEvent'"/>
      <xsl:element name="Super">
        <xsl:attribute name="Has" select="'no'" />
      </xsl:element>
      <xsl:element name="Constructor">
        <xsl:element name="Params">
          <xsl:element name="Param">handler</xsl:element>
        </xsl:element>
        <xsl:variable name="constructorBodyElems">
          <xsl:element name="Code">this.handler = handler;</xsl:element>
          <xsl:element name="Code">return this;</xsl:element>
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
            <xsl:element name="Code">if (!this.handler)</xsl:element>
            <xsl:element name="Code">EventList[++EventCtr].Execute();</xsl:element>
            <xsl:element name="Code">else {</xsl:element>
            <xsl:element name="Code">EventUtil.addHandler(document, "keypress", this.handler, this);</xsl:element>
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
      </xsl:element>
    </xsl:element>

    <xsl:element name="Class">
      <xsl:attribute name="ClassName" select="'IATSubmitEvent'"/>
      <xsl:element name="Super">
        <xsl:attribute name="Has" select="'yes'" />
        <xsl:value-of select="'IATEvent'" />
      </xsl:element>
      <xsl:element name="Constructor">
        <xsl:element name="Params" />
        <xsl:variable name="constructorBodyElems">
          <xsl:element name="Code">IATEvent.call(this, null);</xsl:element>
          <xsl:element name="Code">return this;</xsl:element>
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
            <xsl:element name="Code">Display.getDivTag().appendChild(numItemsInput);</xsl:element>
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
      <xsl:element name="Super">
        <xsl:attribute name="Has" select="'yes'" />
        <xsl:value-of select="'IATEvent'" />
      </xsl:element>
      <xsl:element name="Constructor">
        <xsl:element name="Params">
          <xsl:element name="Param">eventNum</xsl:element>
          <xsl:element name="Param">stimulus</xsl:element>
          <xsl:element name="Param">itemNum</xsl:element>
          <xsl:element name="Param">keyedDir</xsl:element>
          <xsl:element name="Param">blockNum</xsl:element>
        </xsl:element>
        <xsl:variable name="constructorBodyElems">
          <xsl:element name="Code">this.eventNum = eventNum;</xsl:element>
          <xsl:element name="Code">this.keyedDir = keyedDir;</xsl:element>
          <xsl:element name="Code">this.blockNum = blockNum;</xsl:element>
          <xsl:element name="Code">if (keyedDir == "Left")</xsl:element>
          <xsl:element name="Code">IATEvent.call(this, IATItem.prototype.LeftKeyedResponse);</xsl:element>
          <xsl:element name="Code">else</xsl:element>
          <xsl:element name="Code">IATEvent.call(this, IATItem.prototype.RightKeyedResponse);</xsl:element>
          <xsl:element name="Code">this.isErrorMarked = false;</xsl:element>
          <xsl:element name="Code">this.stimulus = stimulus;</xsl:element>
          <xsl:element name="Code">this.itemNum = itemNum;</xsl:element>
          <xsl:element name="Code">return this;</xsl:element>
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
          <xsl:attribute name="FunctionName" select="'LeftKeyedResponse'" />
          <xsl:element name="Params">
            <xsl:element name="Param">event</xsl:element>
          </xsl:element>
          <xsl:variable name="functionBodyElems">
            <xsl:element name="Code">event = EventUtil.getEvent(event);</xsl:element>
            <xsl:element name="Code">EventUtil.stopPropogation(event);</xsl:element>
            <xsl:element name="Code">EventUtil.preventDefault(event);</xsl:element>
            <xsl:element name="Code">var keyCode = EventUtil.getCharCode(event);</xsl:element>
            <xsl:element name="Code">if ((keyCode == Display.getLeftResponseKeyCodeUpper()) || (keyCode == Display.getLeftResponseKeyCodeLower())) {</xsl:element>
            <xsl:element name="Code">EventUtil.removeHandler(document, "keypress", IATItem.prototype.LeftKeyedResponse, this);</xsl:element>
            <xsl:element name="Code">if (!inPracticeBlock) {</xsl:element>
            <xsl:element name="Code">var latency = Display.StopTimer();</xsl:element>
            <xsl:element name="Code">var divTag = document.getElementById("Stimulus" + itemCtr.toString().trim());</xsl:element>
            <xsl:element name="Code">var elem = document.createElement("input");</xsl:element>
            <xsl:element name="Code">elem.setAttribute("type", "hidden");</xsl:element>
            <xsl:element name="Code">elem.setAttribute("name", "Item" + itemCtr.toString().trim());</xsl:element>
            <xsl:element name="Code">elem.setAttribute("id", "Item" + itemCtr.toString().trim());</xsl:element>
            <xsl:element name="Code">elem.setAttribute("value", this.itemNum.toString());</xsl:element>
            <xsl:element name="Code">divTag.appendChild(elem);</xsl:element>
            <xsl:element name="Code">elem = document.createElement("input");</xsl:element>
            <xsl:element name="Code">elem.setAttribute("type", "hidden");</xsl:element>
            <xsl:element name="Code">elem.setAttribute("name", "Latency" + itemCtr.toString().trim());</xsl:element>
            <xsl:element name="Code">elem.setAttribute("id", "Latency" + itemCtr.toString().trim());</xsl:element>
            <xsl:element name="Code">elem.setAttribute("value", latency.toString());</xsl:element>
            <xsl:element name="Code">divTag.appendChild(elem);</xsl:element>
            <xsl:element name="Code">elem = document.createElement("input");</xsl:element>
            <xsl:element name="Code">elem.setAttribute("type", "hidden");</xsl:element>
            <xsl:element name="Code">elem.setAttribute("name", "Block" + itemCtr.toString().trim());</xsl:element>
            <xsl:element name="Code">elem.setAttribute("id", "Block" + itemCtr.toString().trim());</xsl:element>
            <xsl:element name="Code">elem.setAttribute("value", (this.blockNum + 1).toString());</xsl:element>
            <xsl:element name="Code">divTag.appendChild(elem);</xsl:element>
            <xsl:element name="Code">itemCtr++;</xsl:element>
            <xsl:element name="Code">}</xsl:element>
            <xsl:element name="Code">if (isErrorMarked)</xsl:element>
            <xsl:element name="Code">Display.RemoveDisplayItem(ErrorMark);</xsl:element>
            <xsl:element name="Code">isErrorMarked = false;</xsl:element>
            <xsl:element name="Code">Display.RemoveDisplayItem(this.stimulus);</xsl:element>
            <xsl:element name="Code">Display.StartTimer();</xsl:element>
            <xsl:element name="Code">setTimeout(EventList[++EventCtr].Execute(), 100);</xsl:element>
            <xsl:element name="Code">} else if ((keyCode == Display.getRightResponseKeyCodeUpper()) || (keyCode == Display.getRightResponseKeyCodeLower())) {</xsl:element>
            <xsl:element name="Code">if (!isErrorMarked) {</xsl:element>
            <xsl:element name="Code">var errorTag = document.getElementById("Error" + itemCtr.toString().trim());</xsl:element>
            <xsl:element name="Code">errorTag.setAttribute("value", "true");</xsl:element>
            <xsl:element name="Code">Display.AddDisplayItem(ErrorMark);</xsl:element>
            <xsl:element name="Code">isErrorMarked = true;</xsl:element>
            <xsl:element name="Code">}</xsl:element>
            <xsl:element name="Code">}</xsl:element>
          </xsl:variable>
          <xsl:element name="FunctionBody">
            <xsl:for-each select="$functionBodyElems/Code">
              <xsl:if test="string-length(normalize-space(.)) gt 0">
                <xsl:element name="Code">
                  <xsl:value-of select="normalize-space(.)"/>
                </xsl:element>
              </xsl:if>
            </xsl:for-each>
          </xsl:element>
        </xsl:element>

        <xsl:element name="Function">
          <xsl:attribute name="FunctionName" select="'RightKeyedResponse'" />
          <xsl:element name="Params">
            <xsl:element name="Param">event</xsl:element>
          </xsl:element>
          <xsl:variable name="functionBodyElems">
            <xsl:element name="Code">event = EventUtil.getEvent(event);</xsl:element>
            <xsl:element name="Code">EventUtil.stopPropogation(event);</xsl:element>
            <xsl:element name="Code">EventUtil.preventDefault(event);</xsl:element>
            <xsl:element name="Code">var keyCode = EventUtil.getCharCode(event);</xsl:element>
            <xsl:element name="Code">if ((keyCode == Display.getRightResponseKeyCodeUpper()) || (keyCode == Display.getRightResponseKeyCodeLower())) {</xsl:element>
            <xsl:element name="Code">EventUtil.removeHandler(document, "keypress", IATItem.prototype.RightKeyedResponse, this);</xsl:element>
            <xsl:element name="Code">if (!inPracticeBlock) {</xsl:element>
            <xsl:element name="Code">var latency = Display.StopTimer();</xsl:element>
            <xsl:element name="Code">var divTag = document.getElementById("Stimulus" + itemCtr.toString().trim());</xsl:element>
            <xsl:element name="Code">var elem = document.createElement("input");</xsl:element>
            <xsl:element name="Code">elem.setAttribute("type", "hidden");</xsl:element>
            <xsl:element name="Code">elem.setAttribute("name", "Item" + itemCtr.toString().trim());</xsl:element>
            <xsl:element name="Code">elem.setAttribute("id", "Item" + itemCtr.toString().trim());</xsl:element>
            <xsl:element name="Code">elem.setAttribute("value", this.itemNum.toString());</xsl:element>
            <xsl:element name="Code">divTag.appendChild(elem);</xsl:element>
            <xsl:element name="Code">elem = document.createElement("input");</xsl:element>
            <xsl:element name="Code">elem.setAttribute("type", "hidden");</xsl:element>
            <xsl:element name="Code">elem.setAttribute("name", "Latency" + itemCtr.toString().trim());</xsl:element>
            <xsl:element name="Code">elem.setAttribute("id", "Latency" + itemCtr.toString().trim());</xsl:element>
            <xsl:element name="Code">elem.setAttribute("value", latency.toString());</xsl:element>
            <xsl:element name="Code">divTag.appendChild(elem);</xsl:element>
            <xsl:element name="Code">elem = document.createElement("input");</xsl:element>
            <xsl:element name="Code">elem.setAttribute("type", "hidden");</xsl:element>
            <xsl:element name="Code">elem.setAttribute("name", "Block" + itemCtr.toString().trim());</xsl:element>
            <xsl:element name="Code">elem.setAttribute("id", "Block" + itemCtr.toString().trim());</xsl:element>
            <xsl:element name="Code">elem.setAttribute("value", (this.blockNum + 1).toString());</xsl:element>
            <xsl:element name="Code">divTag.appendChild(elem);</xsl:element>
            <xsl:element name="Code">Display.getDivTag().appendChild(divTag);</xsl:element>
            <xsl:element name="Code">itemCtr++;</xsl:element>
            <xsl:element name="Code">}</xsl:element>
            <xsl:element name="Code">if (isErrorMarked)</xsl:element>
            <xsl:element name="Code">Display.RemoveDisplayItem(ErrorMark);</xsl:element>
            <xsl:element name="Code">isErrorMarked = false;</xsl:element>
            <xsl:element name="Code">Display.RemoveDisplayItem(this.stimulus);</xsl:element>
            <xsl:element name="Code">this.handler = null;</xsl:element>
            <xsl:element name="Code">Display.StartTimer();</xsl:element>
            <xsl:element name="Code">setTimeout(EventList[++EventCtr].Execute(), 100);</xsl:element>
            <xsl:element name="Code">} else if ((keyCode == Display.getLeftResponseKeyCodeUpper()) || (keyCode == Display.getLeftResponseKeyCodeLower())) {</xsl:element>
            <xsl:element name="Code">if (!isErrorMarked) {</xsl:element>
            <xsl:element name="Code">var errorTag = document.getElementById("Error" + itemCtr.toString().trim());</xsl:element>
            <xsl:element name="Code">errorTag.setAttribute("value", "true");</xsl:element>
            <xsl:element name="Code">Display.AddDisplayItem(ErrorMark);</xsl:element>
            <xsl:element name="Code">isErrorMarked = true;</xsl:element>
            <xsl:element name="Code">}</xsl:element>
            <xsl:element name="Code">}</xsl:element>
          </xsl:variable>
          <xsl:element name="FunctionBody">
            <xsl:for-each select="$functionBodyElems/Code">
              <xsl:if test="string-length(normalize-space(.)) gt 0">
                <xsl:element name="Code">
                  <xsl:value-of select="normalize-space(.)"/>
                </xsl:element>
              </xsl:if>
            </xsl:for-each>
          </xsl:element>
        </xsl:element>

        <xsl:element name="Function">
          <xsl:attribute name="FunctionName" select="'Execute'"/>
          <xsl:element name="Params"/>
          <xsl:variable name="functionBodyElems">
            <xsl:element name="Code">currentItemKeyedDir = this.keyedDir;</xsl:element>
            <xsl:element name="Code">currentStimulus = this.stimulus;</xsl:element>
            <xsl:element name="Code">currentItemNum = this.itemNum;</xsl:element>
            <xsl:element name="Code">var divElem = document.createElement("div");</xsl:element>
            <xsl:element name="Code">divElem.setAttribute("id", "Stimulus" + itemCtr.toString().trim());</xsl:element>
            <xsl:element name="Code">var elem = document.createElement("input");</xsl:element>
            <xsl:element name="Code">elem.setAttribute("type", "hidden");</xsl:element>
            <xsl:element name="Code">elem.setAttribute("name", "Error" + itemCtr.toString().trim());</xsl:element>
            <xsl:element name="Code">elem.setAttribute("id", "Error" + itemCtr.toString().trim());</xsl:element>
            <xsl:element name="Code">elem.setAttribute("value", "false");</xsl:element>
            <xsl:element name="Code">divElem.appendChild(elem);</xsl:element>
            <xsl:element name="Code">Display.getDivTag().appendChild(divElem);</xsl:element>
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
      <xsl:attribute name="ClassName" select="'IATBeginBlock'"/>
      <xsl:element name="Super">
        <xsl:attribute name="Has" select="'yes'" />
        <xsl:value-of select="'IATEvent'" />
      </xsl:element>
      <xsl:element name="Constructor">
        <xsl:element name="Params">
          <xsl:element name="Param">isPracticeBlock</xsl:element>
          <xsl:element name="Param">leftDisplayItem</xsl:element>
          <xsl:element name="Param">rightDisplayItem</xsl:element>
          <xsl:element name="Param">instructionsDisplayItem</xsl:element>
        </xsl:element>
        <xsl:variable name="constructorBodyElems">
          <xsl:element name="Code">IATEvent.call(this, null);</xsl:element>
          <xsl:element name="Code">this.isPracticeBlock = isPracticeBlock;</xsl:element>
          <xsl:element name="Code">this.leftDisplayItem = leftDisplayItem;</xsl:element>
          <xsl:element name="Code">this.rightDisplayItem = rightDisplayItem;</xsl:element>
          <xsl:element name="Code">this.instructionsDisplayItem = instructionsDisplayItem;</xsl:element>
          <xsl:element name="Code">return this;</xsl:element>
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
      <xsl:element name="Super">
        <xsl:attribute name="Has" select="'yes'" />
        <xsl:value-of select="'IATEvent'" />
      </xsl:element>
      <xsl:element name="Constructor">
        <xsl:element name="Params" />
        <xsl:variable name="constructorBodyElems">
          <xsl:element name="Code">IATEvent.call(this, null);</xsl:element>
          <xsl:element name="Code">return this;</xsl:element>
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
      <xsl:element name="Super">
        <xsl:attribute name="Has" select="'yes'" />
        <xsl:value-of select="'IATEvent'" />
      </xsl:element>
      <xsl:element name="Constructor">
        <xsl:element name="Params">
          <xsl:element name="Param">continueChar</xsl:element>
          <xsl:element name="Param">continueInstructionsDI</xsl:element>
        </xsl:element>
        <xsl:variable name="constructorBodyElems">
          <xsl:element name="Code">IATEvent.call(this, IATInstructionScreen.prototype.keyPressHandler);</xsl:element>
          <xsl:element name="Code">this.continueChar = continueChar;</xsl:element>
          <xsl:element name="Code">this.continueInstructionsDI = continueInstructionsDI;</xsl:element>
          <xsl:element name="Code">return this;</xsl:element>
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
          <xsl:attribute name="FunctionName" select="'keyPressHandler'" />
          <xsl:element name="Params">
            <xsl:element name="Param">event</xsl:element>
          </xsl:element>
          <xsl:variable name="functionBodyElems">
            <xsl:element name="Code">event = EventUtil.getEvent(event);</xsl:element>
            <xsl:element name="Code">EventUtil.stopPropogation(event);</xsl:element>
            <xsl:element name="Code">EventUtil.preventDefault(event);</xsl:element>
            <xsl:element name="Code">var keyCode = EventUtil.getCharCode(event);</xsl:element>
            <xsl:element name="Code">if (keyCode == this.continueChar) {</xsl:element>
            <xsl:element name="Code">EventUtil.removeHandler(document, "keypress", IATInstructionScreen.prototype.keyPressHandler, this);</xsl:element>
            <xsl:element name="Code">Display.Clear();</xsl:element>
            <xsl:element name="Code">this.handler = null;</xsl:element>
            <xsl:element name="Code">setTimeout(EventList[++EventCtr].Execute(), 100);</xsl:element>
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
          <xsl:attribute name="FunctionName" select="'Execute'"/>
          <xsl:variable name="functionBodyElems">
            <xsl:element name="Code">Display.AddDisplayItem(this.continueInstructionsDI);</xsl:element>
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
      <xsl:attribute name="ClassName" select="'IATTextInstructionScreen'"/>
      <xsl:element name="Super">
        <xsl:attribute name="Has" select="'yes'" />
        <xsl:value-of select="'IATInstructionScreen'" />
      </xsl:element>
      <xsl:element name="Constructor">
        <xsl:element name="Params">
          <xsl:element name="Param">continueChar</xsl:element>
          <xsl:element name="Param">continueInstructionsDI</xsl:element>
          <xsl:element name="Param">textInstructionsDI</xsl:element>
        </xsl:element>
        <xsl:variable name="constructorBodyElems">
          <xsl:element name="Code">IATInstructionScreen.call(this, continueChar, continueInstructionsDI);</xsl:element>
          <xsl:element name="Code">this.textInstructionsDI = textInstructionsDI;</xsl:element>
          <xsl:element name="Code">return this;</xsl:element>
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
      <xsl:element name="Super">
        <xsl:attribute name="Has" select="'yes'" />
        <xsl:value-of select="'IATInstructionScreen'" />
      </xsl:element>
      <xsl:element name="Constructor">
        <xsl:element name="Params">
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
          <xsl:element name="Code">IATInstructionScreen.call(this, continueChar, continueInstructionsDI);</xsl:element>
          <xsl:element name="Code">this.leftResponseDI = leftResponseDI;</xsl:element>
          <xsl:element name="Code">this.rightResponseDI = rightResponseDI;</xsl:element>
          <xsl:element name="Code">this.stimulusDI = stimulusDI;</xsl:element>
          <xsl:element name="Code">this.instructionsDI = instructionsDI;</xsl:element>
          <xsl:element name="Code">this.errorMarked = errorMarked;</xsl:element>
          <xsl:element name="Code">this.outlineLeftResponse = outlineLeftResponse;</xsl:element>
          <xsl:element name="Code">this.outlineRightResponse = outlineRightResponse;</xsl:element>
          <xsl:element name="Code">return this;</xsl:element>
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
      <xsl:attribute name="ClassName" select="'IATKeyedInstructionScreen'"/>
      <xsl:element name="Super">
        <xsl:attribute name="Has" select="'yes'" />
        <xsl:value-of select="'IATInstructionScreen'" />
      </xsl:element>
      <xsl:element name="Constructor">
        <xsl:element name="Params">
          <xsl:element name="Param">continueChar</xsl:element>
          <xsl:element name="Param">continueInstructionsDI</xsl:element>
          <xsl:element name="Param">instructionsDI</xsl:element>
          <xsl:element name="Param">leftResponseDI</xsl:element>
          <xsl:element name="Param">rightResponseDI</xsl:element>
        </xsl:element>
        <xsl:variable name="constructorBodyElems">
          <xsl:element name="Code">IATInstructionScreen.call(this, continueChar, continueInstructionsDI);</xsl:element>
          <xsl:element name="Code">this.instructionsDI = instructionsDI;</xsl:element>
          <xsl:element name="Code">this.leftResponseDI = leftResponseDI;</xsl:element>
          <xsl:element name="Code">this.rightResponseDI = rightResponseDI;</xsl:element>
          <xsl:element name="Code">return this;</xsl:element>
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
      <xsl:attribute name="ClassName" select="'IATBlock'"/>
      <xsl:element name="Super">
        <xsl:attribute name="Has" select="'no'" />
      </xsl:element>
      <xsl:element name="Constructor">
        <xsl:element name="Params">
          <xsl:element name="Param">blockNum</xsl:element>
          <xsl:element name="Param">blockPosition</xsl:element>
          <xsl:element name="Param">numPresentations</xsl:element>
          <xsl:element name="Param">alternatedWith</xsl:element>
        </xsl:element>
        <xsl:variable name="constructorBodyElems">
          <xsl:element name="Code">this.blockNum = blockNum;</xsl:element>
          <xsl:element name="Code">this.blockPosition = blockPosition;</xsl:element>
          <xsl:element name="Code">this.numPresentations = numPresentations;</xsl:element>
          <xsl:element name="Code">this.alternatedWith = alternatedWith;</xsl:element>
          <xsl:element name="Code">this.BeginBlockEvent = null;</xsl:element>
          <xsl:element name="Code">this.EndBlockEvent = null;</xsl:element>
          <xsl:element name="Code">this.Items = new Array();</xsl:element>
          <xsl:element name="Code">return this;</xsl:element>
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
          <xsl:element name="FunctionBody">
            <xsl:element name="Code">this.Items.push(item);</xsl:element>
          </xsl:element>
        </xsl:element>

        <xsl:element name="Function">
          <xsl:attribute name="FunctionName" select="'setBeginBlockEvent'" />
          <xsl:element name="Params">
            <xsl:element name="Param">event</xsl:element>
          </xsl:element>
          <xsl:element name="FunctionBody">
            <xsl:element name="Code">this.BeginBlockEvent = event;</xsl:element>
          </xsl:element>
        </xsl:element>

        <xsl:element name="Function">
          <xsl:attribute name="FunctionName" select="'setEndBlockEvent'" />
          <xsl:element name="Params">
            <xsl:element name="Param">event</xsl:element>
          </xsl:element>
          <xsl:element name="FunctionBody">
            <xsl:element name="Code">this.EndBlockEvent = event;</xsl:element>
          </xsl:element>
        </xsl:element>

        <xsl:element name="Function">
          <xsl:attribute name="FunctionName" select="'getNumItems'" />
          <xsl:element name="Params" />
          <xsl:element name="FunctionBody">
            <xsl:element name="Code">return this.Items.length;</xsl:element>
          </xsl:element>
        </xsl:element>

        <xsl:element name="Function">
          <xsl:attribute name="FunctionName" select="'getItem'" />
          <xsl:element name="Params">
            <xsl:element name="Param">ndx</xsl:element>
          </xsl:element>
          <xsl:element name="FunctionBody">
            <xsl:element name="Code">return this.Items[ndx];</xsl:element>
          </xsl:element>
        </xsl:element>

        <xsl:element name="Function">
          <xsl:attribute name="FunctionName" select="'getBeginBlockEvent'" />
          <xsl:element name="Params" />
          <xsl:element name="FunctionBody">
            <xsl:element name="Code">return this.BeginBlockEvent;</xsl:element>
          </xsl:element>
        </xsl:element>

        <xsl:element name="Function">
          <xsl:attribute name="FunctionName" select="'getEndBlockEvent'" />
          <xsl:element name="Params" />
          <xsl:element name="FunctionBody">
            <xsl:element name="Code">return this.EndBlockEvent;</xsl:element>
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
      <xsl:element name="Super">
        <xsl:attribute name="Has" select="'no'" />
      </xsl:element>
      <xsl:element name="Constructor">
        <xsl:element name="Params">
          <xsl:element name="Param">alternatedWith</xsl:element>
          <xsl:element name="Param">blockPosition</xsl:element>
        </xsl:element>
        <xsl:variable name="constructorBodyElems">
          <xsl:element name="Code">this.alternatedWith = alternatedWith;</xsl:element>
          <xsl:element name="Code">this.blockPosition = blockPosition;</xsl:element>
          <xsl:element name="Code">this.screens = new Array();</xsl:element>
          <xsl:element name="Code">return this;</xsl:element>
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
          <xsl:attribute name="FunctionName" select="'getScreen'" />
          <xsl:element name="Params">
            <xsl:element name="Param">ndx</xsl:element>
          </xsl:element>
          <xsl:element name="FunctionBody">
            <xsl:element name="Code">return this.screens[ndx];</xsl:element>
          </xsl:element>
        </xsl:element>

        <xsl:element name="Function">
          <xsl:attribute name="FunctionName" select="'getNumScreens'" />
          <xsl:element name="Params">
            <xsl:element name="Param">ndx</xsl:element>
          </xsl:element>
          <xsl:element name="FunctionBody">
            <xsl:element name="Code">return this.screens.length;</xsl:element>
          </xsl:element>
        </xsl:element>

        <xsl:element name="Function">
          <xsl:attribute name="FunctionName" select="'AddScreen'"/>
          <xsl:element name="Params">
            <xsl:element name="Param">screenType</xsl:element>
            <xsl:element name="Param">ctorArgAry</xsl:element>
          </xsl:element>
          <xsl:variable name="functionBodyElems">
            <xsl:element name="Code">var screen;</xsl:element>
            <xsl:element name="Code">if (screenType == "Text")</xsl:element>
            <xsl:element name="Code">screen = new IATTextInstructionScreen(ctorArgAry[0], ctorArgAry[1], ctorArgAry[2]);</xsl:element>
            <xsl:element name="Code">else if (screenType == "Keyed")</xsl:element>
            <xsl:element name="Code">screen = new IATKeyedInstructionScreen(ctorArgAry[0], ctorArgAry[1], ctorArgAry[2], ctorArgAry[3], ctorArgAry[4]);</xsl:element>
            <xsl:element name="Code">else</xsl:element>
            <xsl:element name="Code">screen = new IATMockItemInstructionScreen(ctorArgAry[0], ctorArgAry[1], ctorArgAry[2], ctorArgAry[3], ctorArgAry[4], ctorArgAry[5], ctorArgAry[6], ctorArgAry[7], ctorArgAry[8]);</xsl:element>
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

  <xsl:variable name="processItemFunctions" >
    <xsl:call-template name="GenerateProcessItemFunctions" />
  </xsl:variable>


  <xsl:variable name="Functions">

    <xsl:element name="Function">
      <xsl:attribute name="FunctionName" select="'BeginIAT'"/>
      <xsl:element name="Params"/>
      <xsl:variable name="functionBodyElems">
        <xsl:element name="Code">var dDiv = null;</xsl:element>
        <xsl:element name="Code">Display = new IATDisplay();</xsl:element>
        <xsl:element name="Code">dDiv = Display.getDivTag();</xsl:element>
        <xsl:element name="Code">while (dDiv.hasChildNodes())</xsl:element>
        <xsl:element name="Code">dDiv.removeChild(dDiv.firstChild);</xsl:element>
        <xsl:element name="Code">EventUtil.removeHandler(window, "click", BeginIAT);</xsl:element>
        <xsl:element name="Code">InitImages();</xsl:element>
        <xsl:element name="Code">GenerateEventList();</xsl:element>
        <xsl:element name="Code">EventCtr = 0;</xsl:element>
        <xsl:element name="Code">itemCtr = 0;</xsl:element>
        <xsl:element name="Code">EventList[0].Execute();</xsl:element>
        <!--
        <xsl:element name="Code">while (Display.divTag.firstChild)</xsl:element>
        <xsl:element name="Code">Display.divTag.removeChild(Display.divTag.firstChild);</xsl:element>
        <xsl:element name="Code">ClickToStartElement = document.createElement("h4");</xsl:element>
        <xsl:element name="Code">ClickToStartText = document.createTextNode("Click Here to Begin");</xsl:element>
        <xsl:element name="Code">ClickToStartElement.appendChild(ClickToStartText);</xsl:element>
        <xsl:element name="Code">Display.divTag.appendChild(ClickToStartElement);</xsl:element>
        <xsl:call-template name="GenerateEventInit">
          <xsl:with-param name="EventListNode" select="//IATEventList"/>
          <xsl:with-param name="randomization" select="//RandomizationType"/>
        </xsl:call-template>
        <xsl:element name="Code">currentHandler = function() {</xsl:element>
        <xsl:element name="Code">Display.divTag.removeChild(ClickToStartElement);</xsl:element>
        <xsl:element name="Code">EventUtil.removeHandler(Display.divTag, "click", currentHandler);</xsl:element>
        <xsl:element name="Code">EventList[EventCtr].Execute();</xsl:element>
        <xsl:element name="Code">};</xsl:element>
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
        <xsl:element name="Code">});</xsl:element>    -->
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
      <xsl:attribute name="FunctionName" select="'InitImages'"/>
      <xsl:element name="Params"/>
      <xsl:variable name="functionBodyElems">
        <xsl:for-each select="//DisplayItemList/IATDisplayItem">
          <xsl:element name="Code">
            <xsl:value-of select="concat('DI', ID, ' = new IATDI(', ID, ', img', ID, ', ', X, ', ', Y, ', ', Width, ', ', Height, ');')"/>
          </xsl:element>
        </xsl:for-each>
        <xsl:variable name="paramList"
                      select="concat(//IATLayout/InteriorWidth, ', ', //IATLayout/InteriorHeight, ', ', //LeftResponseASCIIKeyCodeUpper, ', ', //LeftResponseASCIIKeyCodeLower, ', ', //RightResponseASCIIKeyCodeUpper, ', ', //RightResponseASCIIKeyCodeLower)"/>
        <xsl:element name="Code">
          <xsl:value-of select="concat('Display = new IATDisplay(', $paramList, ');')"/>
        </xsl:element>
        <xsl:element name="Code">
          <xsl:value-of select="concat('ErrorMark = DI', //ErrorMarkID, ';')"/>
        </xsl:element>
        <!--
        <xsl:element name="Code">
          <xsl:value-of select="concat('ErrorMark.setImgTagID(DI', //ErrorMarkID, '.getImgTagID());')"/>
        </xsl:element>-->
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
        <xsl:element name="Code">EventCtr = 0;</xsl:element>
        <xsl:call-template name="GenerateEventInit" />
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
  </xsl:variable>

  <xsl:template match="ConfigFile">
    <xsl:element name="CodeFile">
      <xsl:element name="VarEntries">
        <xsl:copy-of select="$GlobalAbbreviations" />
      </xsl:element>
      <xsl:element name="Classes">
        <xsl:for-each select="$Classes/Class" >
          <xsl:variable name="nodeName" select="name()" />
          <xsl:element name="{$nodeName}">
            <xsl:for-each select="attribute::*">
              <xsl:copy-of select="." />
            </xsl:for-each>
            <xsl:attribute name="ClassPrefix" select="$classPrefix" />
            <xsl:attribute name="ClassFunctionPrefix" select="$classFunctionPrefix" />
            <xsl:copy-of select="child::*"/>
          </xsl:element>
        </xsl:for-each>
      </xsl:element>
      <xsl:element name="Functions">
        <xsl:for-each select="$processItemFunctions//Function">
          <xsl:variable name="nodeName" select="name()" />
          <xsl:element name="{$nodeName}">
            <xsl:for-each select="attribute::*">
              <xsl:copy-of select="." />
            </xsl:for-each>
            <xsl:attribute name="FunctionPrefix" select="$functionPrefix" />
            <xsl:copy-of select="child::*" />
          </xsl:element>
        </xsl:for-each>
        <xsl:for-each select="$Functions/Function">
          <xsl:variable name="nodeName" select="name()" />
          <xsl:element name="{$nodeName}">
            <xsl:for-each select="attribute::*">
              <xsl:copy-of select="."/>
            </xsl:for-each>
            <xsl:attribute name="FunctionPrefix" select="$functionPrefix" />
            <xsl:copy-of select="child::*" />
          </xsl:element>
        </xsl:for-each>
      </xsl:element>
    </xsl:element>
  </xsl:template>


  <xsl:template name="MaskSpecifierArrayAppend">
    <xsl:param name="item"/>
    <xsl:param name="specifier" />
    <xsl:element name="Code">
      <xsl:value-of select="concat('KeyedDirInput = document.getElementById(&quot;DynamicKey', $specifier/ID, '&quot;).value;')"/>
    </xsl:element>
    <xsl:if test="$item/KeyedDir eq 'DynamicLeft'">
      <xsl:element name="Code">if (KeyedDirInput == "True") {</xsl:element>
      <xsl:element name="Code">MaskItemTrueArray.push(new Array());</xsl:element>
      <xsl:element name="Code">KeyedDir = "Left";</xsl:element>
      <xsl:element name="Code">} else {</xsl:element>
      <xsl:element name="Code">KeyedDir = "Right";</xsl:element>
      <xsl:element name="Code">MaskItemFalseArray.push(new Array());</xsl:element>
      <xsl:element name="Code">}</xsl:element>
    </xsl:if>
    <xsl:if test="$item/KeyedDir eq 'DynamicRight'">
      <xsl:element name="Code">if (KeyedDirInput == "True") {</xsl:element>
      <xsl:element name="Code">MaskItemTrueArray.push(new Array());</xsl:element>
      <xsl:element name="Code">KeyedDir = "Right";</xsl:element>
      <xsl:element name="Code">} else {</xsl:element>
      <xsl:element name="Code">KeyedDir = "Left";</xsl:element>
      <xsl:element name="Code">MaskItemFalseArray.push(new Array());</xsl:element>
      <xsl:element name="Code">}</xsl:element>
    </xsl:if>
    <xsl:variable name="params"
                  select="concat('itemCtr++, DI', $item/StimulusDisplayID, ', KeyedDir, ', $item/ItemNum, ', ', $item/OriginatingBlock, ', ',  $item/BlockNum)"/>
    <xsl:element name="Code">if (KeyedDirInput == "True")</xsl:element>
    <xsl:element name="Code">
      <xsl:value-of select="concat('MaskItemTrueArray[MaskItemTrueArray.length - 1].push(new Array(', $params, '));')" />
    </xsl:element>
    <xsl:element name="Code">else</xsl:element>
    <xsl:element name="Code">
      <xsl:value-of select="concat('MaskItemFalseArray[MaskItemFalseArray.length - 1].push(new Array(', $params, '));')"/>
    </xsl:element>
  </xsl:template>

  <xsl:template name="MaskSpecifierArrayAppendRange">
    <xsl:param name="items"/>
    <xsl:param name="specifier" />
    <xsl:element name="Code">
      <xsl:value-of select="concat('KeyedDirInput = document.getElementById(&quot;DynamicKey', $specifier/ID, '&quot;).value;')"/>
    </xsl:element>
    <xsl:for-each select="$items">
      <xsl:if test="KeyedDir eq 'DynamicLeft'">
        <xsl:element name="Code">if (KeyedDirInput == "True") {</xsl:element>
        <xsl:element name="Code">MaskItemTrueArray.push(new Array());</xsl:element>
        <xsl:element name="Code">KeyedDir = "Left";</xsl:element>
        <xsl:element name="Code">} else {</xsl:element>
        <xsl:element name="Code">KeyedDir = "Right";</xsl:element>
        <xsl:element name="Code">MaskItemFalseArray.push(new Array());</xsl:element>
        <xsl:element name="Code">}</xsl:element>
      </xsl:if>
      <xsl:if test="KeyedDir eq 'DynamicRight'">
        <xsl:element name="Code">if (KeyedDirInput == "True") {</xsl:element>
        <xsl:element name="Code">MaskItemTrueArray.push(new Array());</xsl:element>
        <xsl:element name="Code">KeyedDir = "Right";</xsl:element>
        <xsl:element name="Code">} else {</xsl:element>
        <xsl:element name="Code">KeyedDir = "Left";</xsl:element>
        <xsl:element name="Code">MaskItemFalseArray.push(new Array());</xsl:element>
        <xsl:element name="Code">}</xsl:element>
      </xsl:if>
      <xsl:variable name="params"
                    select="concat('itemCtr++, DI', StimulusDisplayID, ', KeyedDir, ', ItemNum, ', ', OriginatingBlock, ', ', BlockNum)"/>
      <xsl:element name="Code">if (KeyedDirInput == "True")</xsl:element>
      <xsl:element name="Code">
        <xsl:value-of select="concat('MaskItemTrueArray[MaskItemTrueArray.length - 1].push(new Array(', $params, '));')" />
      </xsl:element>
      <xsl:element name="Code">else</xsl:element>
      <xsl:element name="Code">
        <xsl:value-of select="concat('MaskItemFalseArray[MaskItemFalseArray.length - 1].push(new Array(', $params, '));')"/>
      </xsl:element>
    </xsl:for-each>
  </xsl:template>

  <xsl:template name="GenerateEventInit">
    <xsl:element name="Code">var iatBlock, instructionBlock, IATBlocks = new Array(), InstructionBlocks = new Array(), NumItemsAry = new Array(), piFunctions = new Array(), pifAry, ctr, ctr2, ctr3, randomNum, sourceAry = 1, iatItem, lesserAry, greaterAry, bAlternate, itemBlockCtr, instructionBlockCtr, itemBlockOrder, instructionBlockOrder, ndx;</xsl:element>
    <xsl:element name="Code">bAlternate = (CookieUtil.get("Alternate") == "yes") ? true : false;</xsl:element>
    <xsl:for-each select="//IATEventList/IATEvent[@EventType eq 'BeginIATBlock']">
      <xsl:variable name="blockPosition" select="count(preceding-sibling::IATEvent[(@EventType eq 'BeginIATBlock') or (@EventType eq 'BeginInstructionBlock')])" />
      <xsl:variable name="numItems" select="NumItems" />
      <xsl:variable name="blockItems" select="following-sibling::IATEvent[position() le xs:integer($numItems)]" />
      <xsl:if test="//RandomizationType eq 'SetNumberOfPresentations'" >
        <xsl:element name="Code">
          <xsl:value-of select="concat('NumItemsAry.push(', NumPresentations, ');')" />
        </xsl:element>
      </xsl:if>
      <xsl:if test="//RandomizationType ne 'SetNumberOfPresentations'" >
        <xsl:element name="Code">
          <xsl:value-of select="concat('NumItemsAry.push(', NumItems, ');')" />
        </xsl:element>
      </xsl:if>
      <xsl:element name="Code">pifAry = new Array();</xsl:element>
      <xsl:element name="Code">piFunctions.push(pifAry);</xsl:element>
      <xsl:variable name="blockNum" select="BlockNum" />
      <xsl:for-each-group select="//DynamicSpecifiers/DynamicSpecifier" group-by="SurveyName">
        <xsl:variable name="surveyNum" select="position()" />
        <xsl:for-each-group select="current-group()" group-by="ItemNum">
          <xsl:if test="count(current-group()) eq 1">
            <xsl:if test="some $item in $blockItems satisfies $item/SpecifierID eq ID">
              <xsl:element name="Code">
                <xsl:value-of select="concat('pifAry.push(PDIF', $blockNum, '_', $surveyNum, '_', ItemNum, ');')" />
              </xsl:element>
            </xsl:if>
          </xsl:if>
          <xsl:if test="count(current-group()) gt 1">
            <xsl:if test="some $item in $blockItems satisfies (some $id in current-group()/ID satisfies $item/SpecifierID eq $id)" >
              <xsl:element name="Code">
                <xsl:value-of select="concat('pifAry.push(PDIF', $blockNum, '_', $surveyNum, '_', current-group()[1]/ItemNum, ');')" />
              </xsl:element>
            </xsl:if>
          </xsl:if>
        </xsl:for-each-group>
      </xsl:for-each-group>
      <xsl:if test="some $item in $blockItems satisfies $item/SpecifierID eq '-1'">
        <xsl:element name="Code">
          <xsl:value-of select="concat('pifAry.push(PIF', $blockNum, ');')" />
        </xsl:element>
      </xsl:if>
      <xsl:choose>
        <xsl:when test="(//Is7Block eq 'True') and (//RandomizationType eq 'SetNumberOfPresentations')">
          <xsl:element name="Code">
            <xsl:value-of select="concat('iatBlock = new IATBlock(', BlockNum, ', ', $blockPosition, ', ', NumPresentations, ', ', AlternatedWith, ');')"/>
          </xsl:element>
        </xsl:when>
        <xsl:when test="(//Is7Block eq 'False') and (//RandomizationType eq 'SetNumberOfPresentations')" >
          <xsl:element name="Code">
            <xsl:value-of select="concat('iatBlock = new IATBlock(', BlockNum, ', ', $blockPosition, ', ', NumPresentations, ', ', AlternatedWith, ');')" />
          </xsl:element>
        </xsl:when>
        <xsl:otherwise>
          <xsl:element name="Code">
            <xsl:value-of select="concat('iatBlock = new IATBlock(', BlockNum, ', ', $blockPosition, ', ',  NumItems, ', ', AlternatedWith, ');')" />
          </xsl:element>
        </xsl:otherwise>
      </xsl:choose>
      <xsl:element name="Code">
        <xsl:value-of select="concat('iatBlock.setBeginBlockEvent(new IATBeginBlock(', lower-case(./PracticeBlock), ', DI', ./LeftResponseDisplayID, ', DI', ./RightResponseDisplayID, ', DI', ./InstructionsDisplayID, '));')"/>
      </xsl:element>
      <xsl:element name="Code">IATBlocks.push(iatBlock);</xsl:element>
    </xsl:for-each>
    <xsl:element name="Code">
      <xsl:value-of select="concat('for (ctr = 0; ctr &lt; ', count(//IATEventList/IATEvent[@EventType eq 'BeginIATBlock']), '; ctr++) {')" />
    </xsl:element>
    <xsl:choose>
      <xsl:when test="(//Is7Block eq 'True') and (//RandomizationType eq 'SetNumberOfPresentations')">
        <xsl:element name="Code">Items1 = new Array();</xsl:element>
        <xsl:element name="Code">Items2 = new Array();</xsl:element>
        <xsl:element name="Code">sourceAry = ((sourceAry == 2) || (ctr == 0)) ? 1 : 2;</xsl:element>
      </xsl:when>
      <xsl:otherwise>
        <xsl:element name="Code">Items = new Array();</xsl:element>
      </xsl:otherwise>
    </xsl:choose>
    <xsl:element name="Code">for (ctr2 = 0; ctr2 &lt; piFunctions[ctr].length; ctr2++)</xsl:element>
    <xsl:element name="Code">piFunctions[ctr][ctr2].call();</xsl:element>
    <xsl:element name="Code">if (Items1.length &lt; Items2.length) {</xsl:element>
    <xsl:element name="Code">lesserAry = Items1;</xsl:element>
    <xsl:element name="Code">greaterAry = Items2;</xsl:element>
    <xsl:element name="Code">} else {</xsl:element>
    <xsl:element name="Code">lesserAry = Items2;</xsl:element>
    <xsl:element name="Code">greaterAry = Items1;</xsl:element>
    <xsl:element name="Code">}</xsl:element>
    <xsl:element name="Code">for (ctr2 = 0; ctr2 &lt; NumItemsAry[ctr]; ctr2++) {</xsl:element>
    <xsl:choose>
      <xsl:when test="(//Is7Block eq 'True') and (//RandomizationType eq 'SetNumberOfPresentations')">
        <xsl:element name="Code">if (lesserAry.length == 0)</xsl:element>
        <xsl:element name="Code">iatItem = greaterAry[Math.floor(Math.random() * greaterAry.length)];</xsl:element>
        <xsl:element name="Code">else {</xsl:element>
        <xsl:element name="Code">if (sourceAry == 1) {</xsl:element>
        <xsl:element name="Code">iatItem = Items1[Math.floor(Math.random() * Items1.length)];</xsl:element>
        <xsl:element name="Code">sourceAry = 2;</xsl:element>
        <xsl:element name="Code">} else {</xsl:element>
        <xsl:element name="Code">iatItem = Items2[Math.floor(Math.random() * Items2.length)];</xsl:element>
        <xsl:element name="Code">sourceAry = 1;</xsl:element>
        <xsl:element name="Code">}</xsl:element>
        <xsl:element name="Code">}</xsl:element>
      </xsl:when>
      <xsl:otherwise>
        <xsl:element name="Code">iatItem = Items[Math.floor(Math.random() * Items.length)];</xsl:element>
      </xsl:otherwise>
    </xsl:choose>
    <xsl:element name="Code">IATBlocks[ctr].AddItem(iatItem);</xsl:element>
    <xsl:element name="Code">}</xsl:element>
    <xsl:element name="Code">IATBlocks[ctr].setEndBlockEvent(new IATEndBlock());</xsl:element>
    <xsl:element name="Code">}</xsl:element>
    <xsl:for-each select="//IATEventList/IATEvent[@EventType eq 'BeginInstructionBlock']">
      <xsl:variable name="blockPosition" select="count(preceding-sibling::IATEvent[(@EventType eq 'BeginInstructionBlock') or (@EventType eq 'BeginIATBlock')]) + 1" />
      <xsl:variable name="numScreens" select="xs:integer(NumInstructionScreens)" />
      <xsl:element name="Code">
        <xsl:value-of select="concat('instructionBlock = new IATInstructionBlock(', AlternatedWith, ', ', $blockPosition, ');')" />
      </xsl:element>
      <xsl:for-each select="following-sibling::IATEvent[position() le $numScreens]">
        <xsl:choose>
          <xsl:when test="@EventType eq 'TextInstructionScreen'">
            <xsl:element name="Code">
              <xsl:value-of select="concat('instructionBlock.AddScreen(&quot;Text&quot;, [', ContinueASCIIKeyCode, ', DI', ContinueInstructionsDisplayID, ', DI', InstructionsDisplayID, ']);')" />
            </xsl:element>
          </xsl:when>
          <xsl:when test="@EventType eq 'KeyedInstructionScreen'">
            <xsl:element name="Code">
              <xsl:value-of select="concat('instructionBlock.AddScreen(&quot;Keyed&quot;, [', ContinueASCIIKeyCode, ', DI', ContinueInstructionsDisplayID, ', DI', InstructionsDisplayID, ', DI', LeftResponseDisplayID, ', DI', RightResponseDisplayID, ']);')" />
            </xsl:element>
          </xsl:when>
          <xsl:when test="@EventType eq 'MockItemInstructionScreen'">
            <xsl:element name="Code">
              <xsl:value-of select="concat('instructionBlock.AddScreen(&quot;MockItem&quot;, [', ContinueASCIIKeyCode, ', DI', ContinueInstructionsDisplayID, ', DI', LeftResponseDisplayID, ', DI', RightResponseDisplayID, ', DI', StimulusDisplayID, ', DI', InstructionsDisplayID, ', ', lower-case(ErrorMarkIsDisplayed), ', ', lower-case(OutlineLeftResponse), ', ', lower-case(OutlineRightResponse), ']);')" />
            </xsl:element>
          </xsl:when>
        </xsl:choose>
      </xsl:for-each>
      <xsl:element name="Code">InstructionBlocks.push(instructionBlock);</xsl:element>
    </xsl:for-each>
    <xsl:element name="Code">
      <xsl:variable name="alternationValues" select="string-join(//IATEventList/IATEvent[@EventType eq 'BeginIATBlock']/AlternatedWith, ', ')" />
      <xsl:value-of select="concat('itemBlockOrder = [', $alternationValues, '];')" />
    </xsl:element>
    <xsl:element name="Code">
      <xsl:variable name="alternationValues" select="string-join(//IATEventList/IATEvent[@EventType eq 'BeginInstructionBlock']/AlternatedWith, ', ')" />
      <xsl:value-of select="concat('instructionBlockOrder = [', $alternationValues, '];')"/>
    </xsl:element>
    <xsl:variable name="EventList">
      <xsl:for-each select="//IATEventList/IATEvent[(@EventType eq 'BeginIATBlock') or (@EventType eq 'BeginInstructionBlock')]" >
        <xsl:variable name="altWith" select="AlternatedWith" />
        <xsl:variable name="bType" select="@EventType" />
        <xsl:variable name="blockElems">
          <xsl:copy-of select="." />
          <xsl:if test="$bType eq 'BeginIATBlock'">
            <xsl:for-each select="for $elem in following-sibling::IATEvent[(@EventType eq 'EndIATBlock') or (@EventType eq 'EndInstructionBlock')][1]/preceding-sibling::IATEvent return $elem">
              <xsl:copy-of select="." />
            </xsl:for-each>
          </xsl:if>
          <xsl:if test="$bType eq 'BeginInstructionBlock'">
            <xsl:for-each select="following-sibling::IATEvent[position() le xs:integer(NumInstructionScreens)]">
              <xsl:copy-of select="." />
            </xsl:for-each>
          </xsl:if>
        </xsl:variable>
        <xsl:element name="Block">
          <xsl:attribute name="AlternatedWith" select="$altWith" />
          <xsl:attribute name="BlockType" select="if ($bType eq 'BeginIATBlock') then 'IAT' else 'Instruction'" />
          <xsl:copy-of select="$blockElems" />
        </xsl:element>
      </xsl:for-each>
    </xsl:variable>
    <xsl:for-each select="$EventList/Block">
      <xsl:variable name="blockType" select="@BlockType" />
      <xsl:variable name="ndx" select="count(preceding-sibling::Block[@BlockType eq $blockType])" />
      <xsl:if test="@AlternatedWith eq '-1'">
        <xsl:element name="Code">
          <xsl:value-of select="concat('ndx = ', $ndx, ';')"/>
        </xsl:element>
      </xsl:if>
      <xsl:if test="@AlternatedWith ne '-1'">
        <xsl:element name="Code">if (!bAlternate)</xsl:element>
        <xsl:element name="Code">
          <xsl:value-of select="concat('ndx = ', $ndx, ';')"/>
        </xsl:element>
        <xsl:element name="Code">else</xsl:element>
        <xsl:element name="Code">
          <xsl:value-of select="concat('ndx = ', xs:integer(@AlternatedWith) - 1, ';')"/>
        </xsl:element>
      </xsl:if>
      <xsl:if test="@BlockType eq 'IAT'">
        <xsl:element name="Code">EventList.push(IATBlocks[ndx].getBeginBlockEvent());</xsl:element>
        <xsl:element name="Code">for (ctr2 = 0; ctr2 &lt; IATBlocks[ndx].getNumItems(); ctr2++)</xsl:element>
        <xsl:element name="Code">EventList.push(IATBlocks[ndx].getItem(ctr2));</xsl:element>
        <xsl:element name="Code">EventList.push(IATBlocks[ndx].getEndBlockEvent());</xsl:element>
      </xsl:if>
      <xsl:if test="@BlockType eq 'Instruction'">
        <xsl:element name="Code">for (ctr2 = 0; ctr2 &lt; InstructionBlocks[ndx].getNumScreens(); ctr2++)</xsl:element>
        <xsl:element name="Code">EventList.push(InstructionBlocks[ndx].getScreen(ctr2));</xsl:element>
      </xsl:if>
    </xsl:for-each>
    <xsl:element name="Code">EventList.push(new IATSubmitEvent());</xsl:element>

    <!--
    </xsl:for-each>
        <xsl:variable name="blockPos" select="." />
        <xsl:variable name="block" select="for $i in . to following-sibling::*[1]  to //IATEvent[(@EventType eq 'BeginIATBlock') or (@EventType eq 'BeginInstructionBlock')][$blockPos]/paren
        <xsl:if test="AlternatedWith ne '-1'">
          <xsl:element name="testBlock">
            <xsl:if test="@EventType eq 'BeginIATBlock'">
            <xsl:for-each select="]
            <xsl:copy-of select=""/>
          </xsl:element>
          </xsl:if>
        </xsl:if>
      </xsl:for-each>
    </xsl:variable>
    <xsl:element name="Code">
      <xsl:value-of select="concat('for (ctr = 0; ctr &lt; ', count(//IATEvent[@EventType eq 'BeginIATBlock']) + count(//IATEvent[@EventType eq 'BeginInstructionBlock'), '; ctr++) {')" />
    </xsl:element>
    <xsl:element name="Code">itemBlockCtr = 0;</xsl:element>
    <xsl:element name="Code">instructionBlockCtr = 0;</xsl:element>
    <xsl:element name="Code">if (ctr == IATBlocks.getContentsPosition()) {</xsl:element>
    <xsl:element name="Code">if (bAlternate)</xsl:element>
    <xsl:element name="Code">ndx = (itemBlockOrder[itemBlockCtr++] == -1) ? itemBlockCtr: itemBlockOrder[itemBlockCtr] - 1;</xsl:element>
    <xsl:element name="Code">else</xsl:element>
    <xsl:element name="Code">ndx = itemBlockCtr++;</xsl:element>
    <xsl:element name="Code">EventList.push(IATBlocks[ndx].getBeginBlockEvent());</xsl:element>
    <xsl:element name="Code">for (ctr2 = 0; ctr2 &lt; IATBlocks[ndx].getItems().length; ctr2++)</xsl:element>
    <xsl:element name="Code">EventList.push(IATBlocks[ndx].getItem(ctr2));</xsl:element>
    <xsl:element name="Code">EventList.push(IATBlocks[ndx].getEndBlockEvent());</xsl:element>
    <xsl:element name="Code">} else {</xsl:element>
    <xsl:element name="Code">if (bAlternate)</xsl:element>
    <xsl:element name="Code">ndx = (instructionBlockOrder[instructionBlockCtr++] == -1) ? instructionBlockCtr : instructionBlockOrder[instructionBlockCtr] - 1;</xsl:element>
    <xsl:element name="Code">else</xsl:element>
    <xsl:element name="Code">ndx = instructionBlockCtr++;</xsl:element>
    <xsl:element name="Code">for (ctr2 = 0; ctr2 &lt; InstructionBlocks[ndx].screens.length; ctr2++)</xsl:element>
    <xsl:element name="Code">EventList.push(InstructionBlocks[ndx].screens[ctr2]);</xsl:element>
    -->
  </xsl:template>

  <xsl:template name="WriteVars">
    <xsl:param name="CodeLines" />
    <xsl:element name="Code">
      <xsl:value-of select="concat(string-join(for $i in 1 to count(CodeLines) return replace(CodeLines[$i], '(var(\s+)[A-Za-z_][A-Za-z0-9_]*)(.*)', '$1'), ', '), '&#x0A;')" />
    </xsl:element>
  </xsl:template>

  <xsl:template name="ProcessMaskSpecifiers">
    <xsl:element name="Code">var lesserLen, lesserAry, greaterAry, lesserLen, lesserLen2, lesserAry2, greaterAry2, randomNum, randomNum2, ctr2;</xsl:element>
    <xsl:element name="Code">if (MaskItemTrueArray.length &gt; MaskItemFalseArray.length) {</xsl:element>
    <xsl:element name="Code">lesserLen = MaskItemFalseArray.length;</xsl:element>
    <xsl:element name="Code">lesserAry = MaskItemFalseArray;</xsl:element>
    <xsl:element name="Code">greaterAry = MaskItemTrueArray;</xsl:element>
    <xsl:element name="Code">} else {</xsl:element>
    <xsl:element name="Code">lesserLen = MaskItemTrueArray.length;</xsl:element>
    <xsl:element name="Code">lesserAry = MaskItemTrueArray;</xsl:element>
    <xsl:element name="Code">greaterAry = MaskItemFalseArray;</xsl:element>
    <xsl:element name="Code">}</xsl:element>
    <xsl:element name="Code">if (lesserLen == 0)</xsl:element>
    <xsl:element name="Code">return;</xsl:element>
    <xsl:choose>
      <xsl:when test="(//Is7Block eq 'True') and (//RandomizationType eq 'SetNumberOfPresentations')" >
        <xsl:element name="Code">if (lesserAry[0][0][4] == 1)</xsl:element>
        <xsl:element name="Code">itemBlock = Items1;</xsl:element>
        <xsl:element name="Code">else</xsl:element>
        <xsl:element name="Code">itemBlock = Items2;</xsl:element>
      </xsl:when>
      <xsl:otherwise>
        <xsl:element name="Code">itemBlock = Items;</xsl:element>
      </xsl:otherwise>
    </xsl:choose>
    <xsl:element name="Code">for (ctr = 0; ctr &lt; lesserLen; ctr++) {</xsl:element>
    <xsl:element name="Code">randomNum = Math.floor(Math.random() * greaterAry.length);</xsl:element>
    <xsl:element name="Code">lesserAry2 = (lesserAry[ctr].length &gt; greaterAry[randomNum].length) ? greaterAry[randomNum] : lesserAry[ctr];</xsl:element>
    <xsl:element name="Code">greaterAry2 = (lesserAry[ctr].length &gt; greaterAry[randomNum].length) ? lesserAry[ctr] : greaterAry[randomNum];</xsl:element>
    <xsl:element name="Code">lesserLen2 = lesserAry2.length;</xsl:element>
    <xsl:element name="Code">for (ctr2 = 0; ctr2 &lt; lesserLen2; ctr2++) {</xsl:element>
    <xsl:element name="Code">randomNum2 = Math.floor(Math.random() * greaterAry2.length);</xsl:element>
    <xsl:element name="Code">itemBlock.push(new IATItem(lesserAry2[ctr2][0], lesserAry2[ctr2][1], lesserAry2[ctr2][3], lesserAry2[ctr2][2], lesserAry2[ctr2][5]));</xsl:element>
    <xsl:element name="Code">itemBlock.push(new IATItem(greaterAry2[randomNum2][0], greaterAry2[randomNum2][1], greaterAry2[randomNum2][3], greaterAry2[randomNum2][2], greaterAry2[randomNum2][5]));</xsl:element>
    <xsl:element name="Code">greaterAry2.splice(randomNum2, 1);</xsl:element>
    <xsl:element name="Code">}</xsl:element>
    <xsl:element name="Code">greaterAry.splice(randomNum, 1);</xsl:element>
    <xsl:element name="Code">}</xsl:element>
  </xsl:template>


  <xsl:template name="ProcessNoSpecItems" >
    <xsl:param name="items" />
    <xsl:for-each select="$items" >
      <xsl:variable name="params"
                    select="concat('EventCtr++, DI', ./StimulusDisplayID, ', ', ./ItemNum, ',  &quot;',  KeyedDir, '&quot;, ', ./BlockNum)"/>
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
    </xsl:for-each>
  </xsl:template>

  <xsl:template name="ProcessTrueFalseSpecItems" >
    <xsl:param name="items" />
    <xsl:variable name="specifier" select="//DynamicSpecifier[every $i in $items satisfies $i/SpecifierID eq ID]" />
    <xsl:element name="Code">var randomNum, TrueFalseAry = new Array();</xsl:element>
    <xsl:element name="Code">
      <xsl:value-of select="concat('KeyedDirInput = document.getElementById(&quot;DynamicKey', $specifier/ID, '&quot;).value;')"/>
    </xsl:element>
    <xsl:for-each select="$items" >
      <xsl:if test="KeyedDir eq 'DynamicLeft'">
        <xsl:element name="Code">if (KeyedDirInput == "True")</xsl:element>
        <xsl:element name="Code">KeyedDir = "Left";</xsl:element>
        <xsl:element name="Code">else</xsl:element>
        <xsl:element name="Code">KeyedDir = "Right";</xsl:element>
      </xsl:if>
      <xsl:if test="KeyedDir eq 'DynamicRight'">
        <xsl:element name="Code">if (KeyedDirInput == "False")</xsl:element>
        <xsl:element name="Code">KeyedDir = "Left";</xsl:element>
        <xsl:element name="Code">else</xsl:element>
        <xsl:element name="Code">KeyedDir = "Right";</xsl:element>
      </xsl:if>
      <xsl:variable name="params" select="concat('EventCtr++, DI', StimulusDisplayID, ', ', ItemNum, ', KeyedDir, ', OriginatingBlock, ', ', BlockNum)" />
      <xsl:element name="Code">
        <xsl:value-of select="concat('TrueFalseAry.push(new Array(', $params, '));')" />
      </xsl:element>
    </xsl:for-each>
    <xsl:element name="Code">randomNum = Math.floor(Math.random() * TrueFalseAry.length);</xsl:element>
    <xsl:choose>
      <xsl:when test="(//Is7Block eq 'True') and (//RandomizationType eq 'SetNumberOfPresentations')" >
        <xsl:element name="Code">if (TrueFalseAry[randomNum][4] == 1)</xsl:element>
        <xsl:element name="Code">Items1.push(new IATItem(TrueFalseAry[randomNum][0], TrueFalseAry[randomNum][1], TrueFalseAry[randomNum][2], TrueFalseAry[randomNum][3], TrueFalseAry[randomNum][5]));</xsl:element>
        <xsl:element name="Code">if (TrueFalseAry[randomNum][4] == 2)</xsl:element>
        <xsl:element name="Code">Items2.push(new IATItem(TrueFalseAry[randomNum][0], TrueFalseAry[randomNum][1], TrueFalseAry[randomNum][2], TrueFalseAry[randomNum][3], TrueFalseAry[randomNum][5]));</xsl:element>
      </xsl:when>
      <xsl:otherwise>
        <xsl:element name="Code">Items.push(new IATItem(TrueFalseAry[randomNum][0], TrueFalseAry[randomNum][1], TrueFalseAry[randomNum][2], TrueFalseAry[randomNum][3], TrueFalseAry[randomNum][5]));</xsl:element>
      </xsl:otherwise>
    </xsl:choose>
  </xsl:template>

  <xsl:template name="ProcessRangeSpecItems" >
    <xsl:param name="items" />
    <xsl:variable name="specifier" select="//DynamicSpecifier[every $i in $items satisfies $i/SpecifierID eq ID]" />
    <xsl:element name="Code">
      <xsl:value-of select="concat('KeyedDirInput = document.getElementById(&quot;DynamicKey', $specifier/ID, '&quot;);')"/>
    </xsl:element>
    <xsl:element name="Code">if (KeyedDirInput.value == "Exclude")</xsl:element>
    <xsl:element name="Code">return;</xsl:element>
    <xsl:element name="Code">var RangeItemAry = new Array();</xsl:element>
    <xsl:for-each select="$items">
      <xsl:if test="KeyedDir eq 'DynamicLeft'">
        <xsl:element name="Code">if (KeyedDirInput == "True")</xsl:element>
        <xsl:element name="Code">KeyedDir = "Left";</xsl:element>
        <xsl:element name="Code">else</xsl:element>
        <xsl:element name="Code">KeyedDir = "Right";</xsl:element>
      </xsl:if>
      <xsl:if test="KeyedDir eq 'DynamicRight'">
        <xsl:element name="Code">if (KeyedDirInput == "False")</xsl:element>
        <xsl:element name="Code">KeyedDir = "Right";</xsl:element>
        <xsl:element name="Code">else</xsl:element>
        <xsl:element name="Code">KeyedDir = "Left";</xsl:element>
      </xsl:if>
      <xsl:variable name="params">
        <xsl:value-of select="concat('itemCtr++, DI', StimulusDisplayID, ', ', ItemNum, ', KeyedDir, ', OriginatingBlock, ', ', BlockNum, ');')" />
      </xsl:variable>
      <xsl:element name="Code">
        <xsl:value-of select="concat('RangeItemAry.push(', $params, ');')"/>
      </xsl:element>
    </xsl:for-each>
    <xsl:element name="Code">var randomNum = Math.floor(Math.random() * RangeItemAry.length);</xsl:element>
    <xsl:choose>
      <xsl:when test="(//Is7Block eq 'True') and (//RandomizationType eq 'SetNumberOfPresentations')" >
        <xsl:element name="Code">if (RangeItemAry[randomNum][4] == 1)</xsl:element>
        <xsl:element name="Code">Items1.push(new IATItem(RangeItemAry[randomNum][0], RangeItemAry[randomNum][1], RangeItemAry[randomNum][2], RangeItemAry[randomNum][3], RangeItemAry[randomNum][5]));</xsl:element>
        <xsl:element name="Code">if (RangeItemAry[randomNum][4] == 2)</xsl:element>
        <xsl:element name="Code">Items2.push(new IATItem(RangeItemAry[randomNum][0], RangeItemAry[randomNum][1], RangeItemAry[randomNum][2], RangeItemAry[randomNum][3], RangeItemAry[randomNum][5]));</xsl:element>
      </xsl:when>
      <xsl:otherwise>
        <xsl:element name="Code">Items.push(new IATItem(RangeItemAry[randomNum][0], RangeItemAry[randomNum][1], RangeItemAry[randomNum][3], RangeItemAry[randomNum][2], RangeItemAry[randomNum][5]));</xsl:element>
      </xsl:otherwise>
    </xsl:choose>
  </xsl:template>

  <xsl:template name="ProcessMaskSpecItems" >
    <xsl:param name="items" />
    <xsl:element name="Code">var MaskItemTrueArray = new Array();</xsl:element>
    <xsl:element name="Code">var MaskItemFalseArray = new Array();</xsl:element>
    <xsl:element name="Code">var ctr;</xsl:element>
    <xsl:for-each-group select="$items" group-by="SpecifierID" >
      <xsl:variable name="specificSpecifier" select="//DynamicSpecifier[every $i in current-group() satisfies $i/SpecifierID eq ID]"/>
      <xsl:if test="count(current-group()) eq 1">
        <xsl:call-template name="MaskSpecifierArrayAppend">
          <xsl:with-param name="item" select="current-group()"/>
          <xsl:with-param name="specifier" select="$specificSpecifier" />
        </xsl:call-template>
      </xsl:if>
      <xsl:if test="count(current-group()) gt 1">
        <xsl:call-template name="MaskSpecifierArrayAppendRange">
          <xsl:with-param name="items" select="current-group() "/>
          <xsl:with-param name="specifier" select="$specificSpecifier" />
        </xsl:call-template>
      </xsl:if>
    </xsl:for-each-group>
    <xsl:call-template name="ProcessMaskSpecifiers" />
  </xsl:template>

  <xsl:template name="ProcessSelectionSpecItems" >
    <xsl:param name="items" />
    <xsl:variable name="specifier" select="//DynamicSpecifier[every $i in $items satisfies $i/SpecifierID eq ID]" />
    <xsl:element name="Code">var SelectionStimulusArray = new Array(), ctr, lesser, lesserLen, ndx1, ndx2, itemBlock, SelectedItem;</xsl:element>
    <xsl:element name="Code">
      <xsl:value-of select="concat('SelectedItem = parseInt(document.getElementById(&quot;DynamicKey', $specifier/ID, '&quot;).value, 10) - 1;')"/>
    </xsl:element>
    <xsl:element name="Code">var RandomItem = SelectedItem;</xsl:element>
    <xsl:for-each-group select="$items[SpecifierID eq $specifier/ID]" group-by="SpecifierArg" >
      <xsl:sort select="current-grouping-key()" order="ascending" />
      <xsl:variable name="choiceNum" select="position()" />
      <xsl:element name="Code">SelectionStimulusArray.push(new Array());</xsl:element>
      <xsl:if test="count(current-group()) eq 1" >
        <xsl:variable name="params"
                      select="concat(./SpecifierArg, ', DI', StimulusDisplayID, ', &quot;', KeyedDir, '&quot;, ', ItemNum, ', ', ./OriginatingBlock, ', ', BlockNum)"/>
        <xsl:element name="Code">
          <xsl:value-of select="concat('SelectionStimulusArray[', xs:integer($choiceNum) - 1, '].push(new Array(', $params, '));')" />
        </xsl:element>
      </xsl:if>
      <xsl:if test="count(current-group()) gt 1" >
        <xsl:for-each select="current-group()" >
          <xsl:variable name="params"
                      select="concat(./SpecifierArg, ', DI', StimulusDisplayID, ', &quot;', KeyedDir, '&quot;, ', ItemNum, ', ', ./OriginatingBlock, ', ', BlockNum)"/>
          <xsl:element name="Code">
            <xsl:value-of select="concat('SelectionStimulusArray[', xs:integer($choiceNum) - 1, '].push(new Array(', $params, '));')" />
          </xsl:element>
        </xsl:for-each>
      </xsl:if>
    </xsl:for-each-group>
    <xsl:element name="Code">RandomItem = SelectedItem;</xsl:element>
    <xsl:element name="Code">while (RandomItem == SelectedItem)</xsl:element>
    <xsl:element name="Code">RandomItem = Math.floor(Math.random() * SelectionStimulusArray.length);</xsl:element>
    <xsl:element name="Code">if (SelectionStimulusArray[RandomItem].length &gt; SelectionStimulusArray[SelectedItem].length) {</xsl:element>
    <xsl:element name="Code">lesser = SelectedItem;</xsl:element>
    <xsl:element name="Code">lesserLen = SelectionStimulusArray[SelectedItem].length;</xsl:element>
    <xsl:element name="Code">} else if (SelectionStimulusArray[RandomItem].length &lt;= SelectionStimulusArray[SelectedItem].length) {</xsl:element>
    <xsl:element name="Code">lesser = RandomItem;</xsl:element>
    <xsl:element name="Code">lesserLen = SelectionStimulusArray[RandomItem].length;</xsl:element>
    <xsl:element name="Code">}</xsl:element>
    <xsl:element name="Code">for (ctr = 0; ctr &lt; lesserLen; ctr++) {</xsl:element>
    <xsl:element name="Code">if (lesser == SelectedItem) {</xsl:element>
    <xsl:element name="Code">ndx1 = ctr;</xsl:element>
    <xsl:element name="Code">ndx2 = Math.floor(Math.random() * SelectionStimulusArray[RandomItem].length);</xsl:element>
    <xsl:element name="Code">} else {</xsl:element>
    <xsl:element name="Code">ndx1 = Math.floor(Math.random() * SelectionStimulusArray[SelectedItem].length);</xsl:element>
    <xsl:element name="Code">ndx2 = ctr;</xsl:element>
    <xsl:element name="Code">}</xsl:element>
    <xsl:choose>
      <xsl:when test="(//Is7Block eq 'True') and (//RandomizationType eq 'SetNumberOfPresentations')">
        <xsl:element name="Code">if (SelectionStimulusArray[SelectedItem][ndx1][4] == 1)</xsl:element>
        <xsl:element name="Code">itemBlock = Items1;</xsl:element>
        <xsl:element name="Code">else</xsl:element>
        <xsl:element name="Code">itemBlock = Items2;</xsl:element>
      </xsl:when>
      <xsl:otherwise>
        <xsl:element name="Code">itemBlock = Items;</xsl:element>
      </xsl:otherwise>
    </xsl:choose>
    <xsl:element name="Code">if (SelectionStimulusArray[SelectedItem][ndx1][2] == "DynamicLeft")</xsl:element>
    <xsl:element name="Code">KeyedDir = "Left";</xsl:element>
    <xsl:element name="Code">else</xsl:element>
    <xsl:element name="Code">KeyedDir = "Right";</xsl:element>
    <xsl:element name="Code">itemBlock.push(new IATItem(EventCtr++, SelectionStimulusArray[SelectedItem][ndx1][1], SelectionStimulusArray[SelectedItem][ndx1][3], KeyedDir, SelectionStimulusArray[SelectedItem][ndx1][5]));</xsl:element>
    <xsl:element name="Code">if (SelectionStimulusArray[RandomItem][ndx2][2] == "DynamicLeft")</xsl:element>
    <xsl:element name="Code">KeyedDir = "Right";</xsl:element>
    <xsl:element name="Code">else</xsl:element>
    <xsl:element name="Code">KeyedDir = "Left";</xsl:element>
    <xsl:element name="Code">itemBlock.push(new IATItem(EventCtr++, SelectionStimulusArray[RandomItem][ndx2][1], SelectionStimulusArray[RandomItem][ndx2][3], KeyedDir, SelectionStimulusArray[SelectedItem][ndx1][5]));</xsl:element>
    <xsl:element name="Code">if (lesser == SelectedItem)</xsl:element>
    <xsl:element name="Code">SelectionStimulusArray[RandomItem].splice(ndx2, 1);</xsl:element>
    <xsl:element name="Code">else</xsl:element>
    <xsl:element name="Code">SelectionStimulusArray[SelectedItem].splice(ndx1, 1);</xsl:element>
    <xsl:element name="Code">}</xsl:element>
  </xsl:template>


  <xsl:template name="GenerateProcessItemFunctions">
    <xsl:for-each select="for $i in 1 to count(//IATEvent[@EventType eq 'BeginIATBlock']) return //IATEvent[@EventType eq 'BeginIATBlock'][$i]">
      <xsl:variable name="i" select="count(preceding-sibling::IATEvent[@EventType eq 'BeginIATBlock']) + 1" />
      <xsl:element name="ProcessItemsFunctions">
        <xsl:attribute name="BlockNum" select="$i" />
        <xsl:variable name="blockStart" select="//IATEvent[@EventType eq 'BeginIATBlock'][1 + count(preceding-sibling::IATEvent[@EventType eq 'BeginIATBlock']) eq $i]" />
        <xsl:variable name="items" select="$blockStart/following-sibling::IATEvent[@EventType eq 'IATItem'][position() le xs:integer($blockStart/NumItems)]" />
        <xsl:for-each-group select="//DynamicSpecifier[some $e in $items satisfies $e/SpecifierID eq ID]" group-by="SurveyName" >
          <xsl:variable name="surveyNum" select="position()" />
          <xsl:for-each-group select="current-group()" group-by="ItemNum" >
            <xsl:variable name="surveyItemNum" select="ItemNum" />
            <xsl:variable name="specType" select="if (count(current-group()) gt 1) then @SpecifierType else current-group()[last()]/@SpecifierType" />
            <xsl:element name="Function">
              <xsl:attribute name="FunctionName" select="concat('PDIF', $i, '_', $surveyNum, '_', $surveyItemNum)" />
              <xsl:element name="Params"/>
              <xsl:element name="FunctionBody">
                <xsl:choose>
                  <xsl:when test="$specType eq 'Mask'">
                    <xsl:call-template name="ProcessMaskSpecItems">
                      <xsl:with-param name="items" select="$items[some $spec in current-group() satisfies $spec/ID eq SpecifierID]" />
                    </xsl:call-template>
                  </xsl:when>

                  <xsl:when test="$specType eq 'Selection'">
                    <xsl:call-template name="ProcessSelectionSpecItems">
                      <xsl:with-param name="items" select="$items[some $spec in current-group() satisfies $spec/ID eq SpecifierID]" />
                    </xsl:call-template>
                  </xsl:when>

                  <xsl:when test="$specType eq 'TrueFalse'">
                    <xsl:call-template name="ProcessTrueFalseSpecItems" >
                      <xsl:with-param name="items" select="$items[some $spec in current-group() satisfies $spec/ID eq SpecifierID]" />
                    </xsl:call-template>l
                  </xsl:when>

                  <xsl:when test="$specType eq 'Range'">
                    <xsl:call-template name="ProcessRangeSpecItems" >
                      <xsl:with-param name="items" select="$items[some $spec in current-group() satisfies $spec/ID eq SpecifierID]" />
                    </xsl:call-template>
                  </xsl:when>
                </xsl:choose>
              </xsl:element>
            </xsl:element>
          </xsl:for-each-group>
        </xsl:for-each-group>
        <xsl:if test="count($items[SpecifierID eq xs:string(-1)]) gt 0">
          <xsl:element name="Function">
            <xsl:attribute name="FunctionName" select="concat('PIF', $i)" />
            <xsl:element name="Params"/>
            <xsl:element name="FunctionBody">
              <xsl:call-template name="ProcessNoSpecItems">
                <xsl:with-param name="items" select="$items[SpecifierID eq xs:string(-1)]" />
              </xsl:call-template>
            </xsl:element>
          </xsl:element>
        </xsl:if>
      </xsl:element>
    </xsl:for-each>
  </xsl:template>

  <xsl:template name="OutputConstructorDefinition">
    <xsl:param name="class"/>
    <xsl:variable name="params" select="$class/Constructor/Params"/>
    <xsl:variable name="paramList" select="string-join($params/Param, ', ')"/>
    <xsl:element name="CodeLine">
      <xsl:attribute name="Type" select="'ConstructorStart'"/>
      <xsl:attribute name="Name" select="$class/@ClassName"/>
      <xsl:value-of select="concat('function ', $class/@ClassName, '(', $paramList, ') {')"/>
    </xsl:element>
  </xsl:template>

  <xsl:template name="OutputConstructorBody">
    <xsl:param name="class"/>
    <xsl:for-each select="$class/Constructor/ConstructorBody/Code">
      <xsl:element name="CodeLine">
        <xsl:attribute name="Type" select="'ConstructorCode'"/>
        <xsl:attribute name="Name" select="$class/@ClassName" />
        <xsl:value-of select="."/>
      </xsl:element>
    </xsl:for-each>
    <xsl:element name="CodeLine">
      <xsl:attribute name="Type" select="'ConstructorEnd'"/>
      <xsl:attribute name="Name" select="$class/@ClassName" />
      <xsl:value-of select="'}'"/>
    </xsl:element>
  </xsl:template>

  <xsl:template name="OutputMemberFunctionDefinition">
    <xsl:param name="function"/>
    <xsl:param name="className"/>
    <xsl:variable name="paramList" select="string-join($function/Params, ', ')"/>
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

  <xsl:template name="OutputFunctionBody" match="FunctionBody[not(DynamicSpecLoad)]" >
    <xsl:for-each select="Code">
      <xsl:element name="CodeLine">
        <xsl:attribute name="Type" select="'FunctionCode'"/>
        <xsl:value-of select=".&#x0A;"/>
      </xsl:element>
    </xsl:for-each>
  </xsl:template>

  <xsl:template name="OutputDynamicSpecFunctionBody" match="FunctionBody[DynamicSpecLoad]" >
    <xsl:for-each select="DynamicSpecLoad">
      <xsl:element name="DynamicSpecLoad">
        <xsl:for-each select="Code">
          <xsl:element name="CodeLine">
            <xsl:attribute name="Type" select="'FunctionCode'"/>
            <xsl:value-of select="."/>
          </xsl:element>
        </xsl:for-each>
      </xsl:element>
    </xsl:for-each>
  </xsl:template>


  <xsl:template name="OutputPrototypeChain">
    <xsl:param name="class"/>
    <xsl:variable name="prototype" select="$class/PrototypeChain"/>
    <xsl:element name="PrototypeChain">
      <xsl:attribute name="NumFunctions" select="count($prototype/Function) + 1" />
      <xsl:element name="CodeLine">
        <xsl:attribute name="Type" select="'ConstructorDefinition'"/>
        <xsl:attribute name="Name" select="$class/@ClassName"/>
        <xsl:value-of select="concat($class/@ClassName, '.prototype.constructor = ', $class/@ClassName, ';')"/>
      </xsl:element>
      <xsl:for-each select="$class/PrototypeChain/Function">
        <xsl:element name="MemberFunction">
          <xsl:attribute name="NumLines" select="count(FunctionBody/Code) + 2" />
          <xsl:call-template name="OutputMemberFunctionDefinition">
            <xsl:with-param name="function" select="."/>
            <xsl:with-param name="className" select="$class/@ClassName"/>
          </xsl:call-template>
          <xsl:apply-templates select="FunctionBody" />
          <xsl:element name="CodeLine">
            <xsl:attribute name="Type" select="'FunctionEnd'"/>
            <xsl:attribute name="Name" select="concat($class/@ClassName, '.', @FunctionName)"/>
            <xsl:value-of select="'};'"/>
          </xsl:element>
        </xsl:element>
      </xsl:for-each>
    </xsl:element>
  </xsl:template>

  <xsl:template name="OutputClass">
    <xsl:param name="class"/>
    <xsl:element name="Class">
      <xsl:attribute name="ClassName" select="$class/@ClassName" />
      <xsl:element name="Constructor">
        <xsl:attribute name="NumLines" select="count($class/Constructor/ConstructorBody/Code) + 2" />
        <xsl:call-template name="OutputConstructorDefinition">
          <xsl:with-param name="class" select="$class"/>
        </xsl:call-template>
        <xsl:call-template name="OutputConstructorBody">
          <xsl:with-param name="class" select="$class"/>
        </xsl:call-template>
      </xsl:element>
      <xsl:call-template name="OutputPrototypeChain">
        <xsl:with-param name="class" select="$class"/>
      </xsl:call-template>
    </xsl:element>
  </xsl:template>

  <xsl:template name="OutputFunction">
    <xsl:param name="function"/>
    <xsl:element name="Function">
      <xsl:attribute name="FunctionName" select="$function/@FunctionName" />
      <xsl:attribute name="NumLines" select="count($function/FunctionBody/Code) + 2" />
      <xsl:call-template name="OutputFunctionDefinition">
        <xsl:with-param name="function" select="."/>
      </xsl:call-template>
      <xsl:apply-templates select="$function/FunctionBody" />
      <xsl:element name="CodeLine">
        <xsl:attribute name="Type" select="'FunctionEnd'"/>
        <xsl:attribute name="Name" select="$function/@FunctionName" />
        <xsl:value-of select="'}'"/>
      </xsl:element>
    </xsl:element>
  </xsl:template>
</xsl:stylesheet>