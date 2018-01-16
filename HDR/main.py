import numpy as np

ASize = 0
ASubSize = 0
curLine = 0
with open("ASize.txt", "r") as ins:
    for line in ins:
        if(curLine==0):
            ASize = int(line)
        else:
            ASubSize = int(line)
        curLine += 1

A = np.zeros(shape=(ASize, ASubSize), dtype=np.float32)
b = np.zeros(shape=(np.size(A, 0), 1), dtype=np.float32)

with open("A.txt", "r") as ins:
    x = 0
    y = 0
    for line in ins:
        line = line.replace("\n","")
        line = line.replace(",",".")
        A[x][y] = float(line)
        y += 1
        if y >= ASubSize:
            y = 0
            x += 1

with open("b.txt", "r") as ins:
    i = 0
    for line in ins:
        line = line.replace("\n","")
        line = line.replace(",",".")
        b[i] = float(line)
        i += 1

x = np.linalg.lstsq(A, b)[0]
for value in x[0:256]:
    print(str(value))

