# therb-gh.  

## テスト手順  
1. モデリングする  
2. それぞれのzoneをsplitしてjoinする  
3. assignClassコンポーネントにzoneジオメトリを格納する  
4. roomの数を入力する  
5. 

## 概要  
THERB-GHは、THERB(HASP)用のシミュレーション入力ファイルをRhinocerosに入力したジオメトリを元に生成するGrasshopperライブラリです。  

THERB-GHはTHERB-webのモデリング部分です。  
![THERB](https://user-images.githubusercontent.com/90674244/142145820-0d25d627-ebec-4a77-b8c4-75bc51a68175.png)　　

## 使い方  
1. 建物をモデリングして、createModelコンポーネントに読み込みます  
2. 気象データを選択します  
3. 建物の仕様を選択します  
4. exportTHERBコンポーネントに接続します  
5. uploadWebコンポーネントに接続します  
6. 5がuploadされたデータのビジュアライゼーションページのurlを返すので、そのリンクにとびます  
