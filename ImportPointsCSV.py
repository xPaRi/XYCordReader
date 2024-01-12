#Author-PaRi
#Description-Import X,Y or X,Y,Z points from CSV

#Example 1
#absX;absY;absZ;relX;relY;relZ
#-43.70;34.90;4.00;0.00;0.00;4.00
#43.70;36.50;4.00;0.00;1.60;4.00
#61.60;53.50;4.00;17.90;18.60;4.00

#Example 2
#17.90,18.60
#29.40,18.60
#29.40,15.60

import adsk.core, adsk.fusion, adsk.cam, traceback

def run(context):
    ui = None
    try:
        app = adsk.core.Application.get()
        ui  = app.userInterface

        # Get all components in the active design.
        product = app.activeProduct
        design = product
        title = 'Import Points csv'
        if not design:
            ui.messageBox('No active Fusion design', title)
            return
        
        # Open file dialog
        dlg = ui.createFileDialog()
        dlg.title = 'Open CSV File'
        dlg.filter = 'Comma Separated Values (*.csv);;All Files (*.*)'
        if dlg.showOpen() != adsk.core.DialogResults.DialogOK :
            return
        
        filename = dlg.filename

        # Open file and read points
        file = open(filename, 'r')
        points = adsk.core.ObjectCollection.create()
        data = []

        for line in file:
            pntStrArr = line.replace(";",",").split(',')

            if len(pntStrArr) < 2:
                continue

            try:
                for pntStr in pntStrArr:
                    data.append(float(pntStr)/10)
            except:
                continue
        
            if len(data) == 2 :
                point = adsk.core.Point3D.create(data[0], data[1], 0)
                points.add(point)
            elif len(data) >= 3 :
                point = adsk.core.Point3D.create(data[0], data[1], data[2])
                points.add(point)
            
            data.clear()

        file.close()

        # Drawing points
        root = design.rootComponent
        sketch = root.sketches.add(root.xYConstructionPlane)

        for point in points:
            sketch.sketchPoints.add(point)

    except:
        if ui:
            ui.messageBox('Failed:\n{}'.format(traceback.format_exc()))

main()