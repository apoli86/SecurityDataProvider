kubectl delete configmap nlog-configmap
kubectl create configmap nlog-configmap --from-file=NLog.config
