import bpy, json, math
import itertools
from bpy import context

# Open script in blender file editor and then use the run script button
# To see script print output you need to run blender from the command line unfortunately

# filepath = "/Users/ds/Documents/blender/export.json"
# filepath = "D:\My Dropbox\stac\maps\export.json"
filepath = r"C:\Users\dillo_000.DILLON\Documents\Programming\centauri-tac\maps\export.json"

obj = context.object
mesh = obj.data

class Tile:
    pass

# XYZ offsets to find the other points on a cube
offsets = [
    [1, 0, 0],
    [1, -1, 0],
    [0, -1, 0],

    [0, 0, -1],
    [1, 0, -1],
    [1, -1, -1],
    [0, -1, -1],
]

with open(filepath, 'w') as f:
    tiles = []
    # pull out just the x,y,z coords from the vertex, and use ceil to make our life easier since they will be converted to ints anyways
    verts = list( map(lambda v: [math.ceil(v.co.x), math.ceil(v.co.y), math.ceil(v.co.z)], mesh.vertices) )

    missingVerts = []
    # first go through all the verts and try to "fill out" any missing vertices
    # in the interior of the mesh where they are removed
    # do this by loking down the z axis to see if there is a lower vertex below
    # this one and verts missing inbetwixt
    for vert in verts:
        matchingXYLower = [d for d in verts if d[0] == vert[0] and d[1] == vert[1] and d[2] < vert[2] ]
        if len(matchingXYLower) == 0:
            continue
        #find min z now and march towards it making sure all verts in between are represented 
        minZ = min(v[2] for v in matchingXYLower)
        for z in range(int(vert[2]), int(minZ), -1):
            newVert = [ vert[0], vert[1], float(z) ]
            if newVert not in verts:
                missingVerts.append(newVert)

    # deduplicate the missing verts because multiple can be added when stepping down through points
    missingVerts.sort()
    missingVerts = list(missingVerts for missingVerts,_ in itertools.groupby(missingVerts))
    print( "Missing Verts")
    print( '[%s]' % ', '.join(map(str, missingVerts)) )

    verts = verts + missingVerts

    print( '[%s]' % ', '.join(map(str, verts)) )
    for vert in verts:
        # print("vert " + str(vert))
        isTopLeftCorner = True
        for o in offsets:
            newVert = [ vert[0] + o[0], vert[1] + o[1], vert[2] + o[2] ]
            if newVert not in verts:
                isTopLeftCorner = False
                break

        # NOTE! The Y and Z axis are switched from blender to unity. The non height axis must be int's as well
        if isTopLeftCorner:
            tiles.append({
                'x': int(vert[0]),
                'y': vert[2],
                'z': int(vert[1])
            })
    
    # dedupe tiles because the vertices are duplicated
    tiles = map(dict, set(tuple(x.items()) for x in tiles))

    tiles = sorted(tiles, key=lambda x: (x['z'], x['y'], x['x']))

    map = {
        'name': 'export',
        'maxPlayers': 2,
        'startingPositions': [],
        'tiles': list(map(lambda v: {'transform': v}, tiles))
    }
    bObjectAsJson = json.dumps(map, indent=4, sort_keys=True)
    f.write(bObjectAsJson)
    print('Done!')