command line:
python getColor.py -x1 2142 -x2 64559 -x3 5595 -x4 1149 -x5 1330 -x6 7508

make installer
pyinstaller --hidden-import="sklearn.neighbors._typedefs" --hidden-import="sklearn.utils._cython_blas" --path C:\projects\github\repos\AviaToolset\ColorDetection\venv\Lib\site-packages\sklearn\.libs getColor.py

