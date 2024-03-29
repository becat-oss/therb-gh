---
slug: class
title: 建築環境設備設計演習 THERB-GHによる熱負荷計算の検討演習  
authors:
  name: katsuya obara
  title: Lead Engineer at BeCAT
  url: https://github.com/katsuya0719
  image_url: https://github.com/katsuya0719.png
tags: [lecture]
---
### 熱負荷計算は何をしているのか?  
室内空間に着目して、時々刻々の**熱収支**を計算している。  
結果として、自然室温や表面温度の変化をシミュレーションで計算することができる  
![heat](heatBalance.png)  

### 授業の目的: THERB-GHを使って簡単な熱負荷計算をハンズオンで行ってみる  
この授業では、尾崎研究室で開発している熱負荷計算用のシミュレーションエンジンTHERBを使います。ただし、このTHERBという計算エンジンは入力用、出力用のGUIがないので初心者がいきなり触り始めるのはおすすめできません。このTHERB-GHを使うことでRhinocerosでモデリングしたジオメトリを使って熱負荷計算をすることができます。  
それでは早速THERB-GHを皆さんのPCにインストールしてもらいます。
環境構築の方法は、[こちらのページ](../../docs/Usage/HowToInstall.md)を使いながら行います。  

### Tutorial 0. THERBモデルを作成する  
環境が構築出来たら、実際にexample.3dm、example.ghファイルを元にTHERBモデルを構築してみます。  
[こちらのページ](../../docs/Usage/CreateTherbModel.md)をみながら、準備してあるジオメトリを使ってシミュレーションを計算、結果を見てみます。  
[結果分析はjupyter notebookへ](https://colab.research.google.com/github/becat-oss/therb-notebook/blob/main/.ipynb_checkpoints/%E6%99%82%E7%B3%BB%E5%88%97%E3%83%87%E3%83%BC%E3%82%BF%E5%88%86%E6%9E%90-checkpoint.ipynb)

### Tuotirial 1. 0からモデルを作ってみよう    
5m×5mの室をモデリングしたあと、南面に窓をモデリングして計算結果を見てみましょう。  
①5m×5mの室（高さ3m)をモデリング  
②南面に2m×2mの大きさの窓をモデリング  
③計算を回してみる  

### Tutorial 2. 窓を変えてみよう  
窓の大きさ、方位角を変えると入射日射量が変化し、室内の熱取得に影響します。どれくらい自然室温が変わるのか、比較してみましょう。  

### Tutorial 3. 換気回数を変えてみよう  
一般的な住宅における必要な換気回数は0.5回/hと定められています。さらに、コロナ禍ということもあり、換気回数を増やすことが奨励されています。一方、換気回数を増やすことで夏であれば、暖かい空気を室内に取り込むこととなるため、室温の増加、空調負荷の増加につながります。適切な換気回数の設定は空気質の確保、空調負荷の最適化の面で重要です。

### Tutorial 4. 壁、屋根、床（壁体構成）の種類を変えてみよう 
壁体構成を変更することで、断熱性、熱容量（室温の安定性に影響）が変わります。どれくらい自然室温が変わるのか、比較してみましょう。  

### Totorial 5. 空調負荷を計算してみよう  
自然室温のままだと、人間が快適に室内で過ごすには夏は暑すぎたり、冬は寒すぎたりします。そこで空調をすることで熱を取り除いたり、加えることによって空気を調和しています。空調負荷とは、自然室温の状態から設定温度（暖房であれば20℃、冷房であれば28℃）にするためにどれくらいの熱量をコントロールしないといけないかという計算をした結果です。  
この空調負荷を実際に取り除くために、設備設計の方々が空調の容量を選定しています。  
省エネな建築を設計するためには、この空調負荷をなるべく小さくするような設計をすることが必要です。  

### 課題. 空調負荷のなるべく少ない窓、壁体構成、換気負荷、方位角の組み合わせを考えてみよう  
制約条件: 
- 建物の容積は100m3(室の数は問わない / 人が入れる空間にしてください)      
- 換気量は全室0.5回/h  

変更可能なパラメータ:   
- 窓の位置、大きさ  
- 壁体構成  
- 方位角  

評価指標:  
快適時間率=>試験的に快適時間率ランキングってのを試してみます。     
合計の空調負荷  
ディグリーデー  

提出レポートの内容:  
以下の内容を記述してください  
- 設計に関して利用した指標(快適時間率/合計の空調負荷 etc)  
- 設計の工夫に関する簡単な説明  
- 計算結果  
- 考察  


### 快適時間率ランキング  
実験的試みとして、快適時間率ランキングというのをしてみます。  
ルール：  
上記課題の条件で、快適時間率が一番高いデザインを目指してください。  
計算結果をアップロードするたび、サーバーで快適時間率を計算しています。  
次回の授業で、快適時間率が最も高いデザインを表彰します。  

快適時間率の定義:  
下限値：18℃  
上限値: 28℃  

:::note
今回使用するシステムはベータ版であり、今後も機能が追加、改訂されていきます。  
結果として、以下のことが起こりえます。  
- 開発途中にデータベースのデータが消える可能性も0ではありません。ですので、シミュレーションデータの管理は各自でもお願いします。  
- 追加した機能についてはテストはしていますが、バグが入っていないとはいいきれません。ですので、何か結果がおかしいなど感じたら遠慮なくkatsuya0719@gmail.comに連絡をお願いします。
:::


### もっと興味ある人はこんなのもおすすめ  
[快適性の判断をしたければこれ](https://comfort.cbe.berkeley.edu/)  

温暖化の影響を検討したければ=>有馬先生に温暖化したあとの気象データをもらってください  






